using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerState))]
public class Player : MonoBehaviour
{
    // --- 컴포넌트 및 컨트롤러 참조 ---
    private PlayerInputHandler inputHandler; // 입력 처리기
    private PlayerMoveController mc;       // 이동 로직 제어
    private PlayerAnimController ac;       // 애니메이션 로직 제어
    private Rigidbody2D rb;                // 물리 컴포넌트
    private Animator anim;                 // 애니메이터
    public PlayerState state;              // 플레이어 스탯 데이터

    // --- 상태 변수 ---
    private Vector2 moveInput;             // 현재 이동 입력 값
    private bool isGrounded;               // 지면에 닿아있는지 여부
    private bool isAttacking;              // 현재 공격 애니메이션 재생 중인지

    [Header("지면 체크 설정")]
    public Transform groundCheckPoint;     // 지면 체크 중심점 (발밑)
    public float groundCheckRadius = 0.2f; // 체크 범위 반지름
    public LayerMask groundLayer;          // 지면으로 인식할 레이어
    public LayerMask platformLayer;        // 플랫폼(발판) 레이어

    [Header("점프 설정")]
    public float jumpForce = 12f;          // 점프 힘

    [Header("대시 설정")]
    public float dashSpeed = 22f;          // 대시 속도
    public float dashDuration = 0.2f;      // 대시 지속 시간
    public float dashCooldown = 0.5f;      // 대시 재사용 대기시간
    public float dashStaminaCost = 20f;    // 대시 시 소모 스태미나
    private bool canDash = true;           // 대시 가능 여부
    private bool isDashing;                // 현재 대시 중인지

    [Header("콤보 설정")]
    public int comboIndex = 0;             // 현재 공격 콤보 순서 (0, 1, 2)
    public float comboResetTime = 0.8f;    // 콤보가 초기화되는 유효 시간
    private float lastAttackTime;          // 마지막으로 공격한 시점 기록

    [Header("콤보 버퍼 설정")]
    public float bufferThreshold = 0.7f;    // 애니메이션이 몇 % 진행됐을 때 다음 입력을 예약할지
    private bool inputBuffered = false;     // 공격 예약 버튼이 눌렸는지 여부 [cite: 2026-02-15]

    private GameObject currentPlatform;    // 현재 딛고 있는 하향 점프용 발판

    private void Awake()
    {
        // 컴포넌트 할당 및 컨트롤러 인스턴스 생성
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        state = GetComponent<PlayerState>();

        mc = new PlayerMoveController(rb);
        ac = new PlayerAnimController(anim);
    }

    private void OnEnable()
    {
        // 입력 이벤트 구독
        inputHandler.OnMove += HandleMove;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnAttack += HandleAttack;
        inputHandler.OnDodge += HandleDodge;
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        inputHandler.OnMove -= HandleMove;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnAttack -= HandleAttack;
        inputHandler.OnDodge -= HandleDodge;
    }

    private void Update()
    {
        bool wasGroundedBefore = isGrounded;
        CheckGround(); // 매 프레임 바닥 체크

        // --- 애니메이터 파라미터 갱신 ---
        // 수직 속도(yVelocity)와 접지 상태(isGrounded)를 전용 컨트롤러에 전달
        ac.UpdateAerialState(isGrounded, rb.linearVelocity.y);

        // --- 착지 시 공격 상태 강제 리셋 ---
        // 공중 공격 중 땅에 닿으면 즉시 공격 상태를 해제하여 이동을 가능하게 함 [cite: 2026-02-15]
        if (!wasGroundedBefore && isGrounded)
        {
            if (isAttacking)
            {
                isAttacking = false;
                inputBuffered = false;
                comboIndex = 0; // 콤보 인덱스도 초기화
            }
        }

        // 공격 중일 때만 입력 버퍼(예약)를 체크하여 다음 연타 실행 [cite: 2026-02-15]
        if (isAttacking)
        {
            CheckComboBuffer();
        }

        // 콤보 유효 시간이 지나면 0타로 리셋
        if (Time.time - lastAttackTime > comboResetTime)
        {
            comboIndex = 0;
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return; // 대시 중에는 물리 이동 로직 무시

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            // 이동 로직 실행 및 스프라이트 방향 전환
            mc.Move(moveInput.x, state.FinalSpd);
            FlipSprite(moveInput.x);

            // 공격 중이 아닐 때만 이동 애니메이션 활성화
            ac.PlayMove(!isAttacking);
        }
        else
        {
            mc.Stop();
            ac.PlayMove(false);
        }
    }

    private void HandleMove(Vector2 dir) => moveInput = dir;

    private void HandleJump()
    {
        // 하향 점프 처리 (S + 점프)
        if (moveInput.y < -0.5f && currentPlatform != null)
        {
            StartCoroutine(DownJump());
        }
        // 일반 점프 (땅 위에서만 가능)
        else if (isGrounded && !isAttacking)
        {
            mc.Jump(jumpForce);
        }
    }

    void HandleAttack()
    {
        // 1. 공중 공격: 무조건 1타만 나가도록 고정하고 연타 방지 [cite: 2026-02-15]
        if (!isGrounded)
        {
            comboIndex = 0;
            ExecuteAttack();
            inputBuffered = false;
            return;
        }

        // 2. 지상 공격: 공격 중이 아니면 실행, 공격 중이면 예약 [cite: 2026-02-15]
        if (!isAttacking) ExecuteAttack();
        else inputBuffered = true;
    }

    void CheckComboBuffer()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 현재 애니메이션이 "Attack" 태그를 가지고 있고, 설정된 임계점 이상 진행되었을 때 [cite: 2026-02-12]
        if (stateInfo.IsTag("Attack"))
        {
            if (stateInfo.normalizedTime >= bufferThreshold && inputBuffered)
            {
                ExecuteAttack(); // 예약된 다음 공격 실행
            }
        }
    }

    void ExecuteAttack()
    {
        inputBuffered = false; // 예약 사용 완료
        isAttacking = true;

        // 애니메이션 컨트롤러를 통해 콤보 인덱스와 트리거 전달
        ac.PlayAttack(comboIndex);

        // 지상에서만 콤보 순환 (0->1->2->0)
        if (isGrounded)
        {
            comboIndex = (comboIndex + 1) % 3;
        }

        lastAttackTime = Time.time; // 마지막 타격 시점 기록
    }

    void HandleDodge()
    {
        // 쿨타임 및 스태미나 체크 후 대시 실행
        if (!canDash || isDashing) return;
        if (state.ConsumeStamina(dashStaminaCost))
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f; // 대시 중에는 중력 무시

        // 입력 방향 혹은 현재 보는 방향으로 대시
        float dashDir = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : transform.localScale.x;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void CheckGround()
    {
        // 발밑에 원형 범위를 생성하여 지면 레이어와 충돌하는지 체크
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer | platformLayer);
    }

    // --- 기즈모 그리기 (유니티 에디터 상에서 범위 확인용) ---
    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;

        // 바닥에 닿아있으면 초록색, 떠있으면 빨간색으로 표시
        Gizmos.color = isGrounded ? Color.green : Color.red;

        // 지면 체크 범위 그리기
        Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
    }

    private IEnumerator DownJump()
    {
        Collider2D platformCol = currentPlatform.GetComponent<Collider2D>();
        CapsuleCollider2D playerCol = GetComponent<CapsuleCollider2D>();

        if (platformCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(playerCol, platformCol, true);
            yield return new WaitForSeconds(0.5f);
            Physics2D.IgnoreCollision(playerCol, platformCol, false);
        }
    }

    private void FlipSprite(float xDir)
    {
        float absX = Mathf.Abs(transform.localScale.x);
        float y = transform.localScale.y;
        if (xDir > 0) transform.localScale = new Vector3(absX, y, 1);
        else if (xDir < 0) transform.localScale = new Vector3(-absX, y, 1);
    }

    // 애니메이션 이벤트(모션 끝)에서 호출하여 다시 자유로운 상태로 복귀
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & platformLayer) != 0) currentPlatform = col.gameObject;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & platformLayer) != 0) currentPlatform = null;
    }
}