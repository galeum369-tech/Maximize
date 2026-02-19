using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerState))]
public class Player : MonoBehaviour, IDamageable
{
    // --- 컴포넌트 및 컨트롤러 참조 ---
    private PlayerInputHandler inputHandler;
    private PlayerMoveController mc;
    private PlayerAnimController ac;
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    public PlayerState state;

    // --- 상태 변수 ---
    private Vector2 moveInput;
    private bool isGrounded;
    private bool isAttacking;
    private Color originalColor;

    [Header("지면 체크 설정")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask platformLayer;

    [Header("점프 설정")]
    public float jumpForce = 12f;

    [Header("대시(충전식 거리 기반) 설정")]
    public float dashDistance = 3f;
    public float dashDuration = 0.2f;
    public int maxDashCount = 2;
    public int currentDashCount;
    public float dashRegenTime = 1.5f;
    public float dashInterval = 0.1f;

    private bool isDashing;
    private bool canDashNext = true;
    private float regenTimer;

    [Header("콤보 설정")]
    public int comboIndex = 0;
    public float comboResetTime = 0.8f;
    private float lastAttackTime;

    [Header("콤보 버퍼 설정")]
    public float bufferThreshold = 0.7f;
    private bool inputBuffered = false;

    private GameObject currentPlatform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        inputHandler = GetComponent<PlayerInputHandler>();
        state = GetComponent<PlayerState>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        mc = new PlayerMoveController(rb);
        ac = new PlayerAnimController(anim);

        currentDashCount = maxDashCount;
    }

    private void OnEnable()
    {
        inputHandler.OnMove += HandleMove;

        // [수정] bool 매개변수를 받는 메서드 연결
        inputHandler.OnJump += HandleJump;
        inputHandler.OnAttack += HandleAttack;

        inputHandler.OnDodge += HandleDodge;
    }

    private void OnDisable()
    {
        inputHandler.OnMove -= HandleMove;

        // [수정] 연결 해제
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnAttack -= HandleAttack;

        inputHandler.OnDodge -= HandleDodge;
    }

    private void Update()
    {
        bool wasGroundedBefore = isGrounded;
        CheckGround();

        ac.UpdateAerialState(isGrounded, rb.linearVelocity.y);

        if (!wasGroundedBefore && isGrounded)
        {
            if (isAttacking)
            {
                isAttacking = false;
                inputBuffered = false;
                comboIndex = 0;
            }
        }

        // 대시 충전 로직
        if (currentDashCount < maxDashCount)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= dashRegenTime)
            {
                currentDashCount++;
                regenTimer = 0f;
            }
        }

        if (isAttacking) CheckComboBuffer();

        if (Time.time - lastAttackTime > comboResetTime) comboIndex = 0;
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            mc.Move(moveInput.x, state.FinalSpd);
            FlipSprite(moveInput.x);
            ac.PlayMove(!isAttacking);
        }
        else
        {
            mc.Stop();
            ac.PlayMove(false);
        }
    }

    public void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
        if (isDashing) return;

        state.currentHp -= damage;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(hitDirection * knockback, ForceMode2D.Impulse);

        StopCoroutine(nameof(FlashRed));
        StartCoroutine(FlashRed());

        if (state.currentHp <= 0) ac.PlayDeath();
    }

    private IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }

    private void HandleMove(Vector2 dir) => moveInput = dir;

    // [수정] bool 값을 받도록 변경
    private void HandleJump(bool pressed)
    {
        if (!pressed) return; // 버튼을 뗐을 때는 무시 (기존 로직 유지)

        if (moveInput.y < -0.5f && currentPlatform != null) StartCoroutine(DownJump());
        else if (isGrounded && !isAttacking) mc.Jump(jumpForce);
    }

    // [수정] bool 값을 받도록 변경 (핵심 로직 수정)
    private void HandleAttack(bool pressed)
    {
        // 1. 버튼을 뗄 때(false)는 동작 안 함 (단타 유지)
        if (!pressed) return;

        // 2. 공중 공격 처리 (단타)
        if (!isGrounded)
        {
            comboIndex = 0;
            ExecuteAttack(); // 즉시 발동
            inputBuffered = false;
            return;
        }

        // 3. 지상 콤보 처리
        // 공격 중이 아니면 -> 바로 1타 발동
        if (!isAttacking)
        {
            ExecuteAttack();
        }
        // 공격 중이면 -> 예약(Buffer)만 걸어둠 (CheckComboBuffer에서 처리)
        else
        {
            inputBuffered = true;
        }
    }

    private void CheckComboBuffer()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        // 예약된 입력이 있고, 애니메이션이 충분히 진행되었을 때 다음 타격 발동
        if (stateInfo.IsTag("Attack") && stateInfo.normalizedTime >= bufferThreshold && inputBuffered)
        {
            ExecuteAttack();
        }
    }

    private void ExecuteAttack()
    {
        inputBuffered = false;
        isAttacking = true;
        ac.PlayAttack(comboIndex);
        if (isGrounded) comboIndex = (comboIndex + 1) % 3;
        lastAttackTime = Time.time;
    }

    private void HandleDodge()
    {
        if (currentDashCount > 0 && canDashNext) StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        currentDashCount--;
        isDashing = true;
        canDashNext = false;
        regenTimer = 0f;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);

        float dashDir = moveInput.x != 0 ? Mathf.Sign(moveInput.x) : Mathf.Sign(transform.localScale.x);
        float dashVel = dashDistance / dashDuration;

        rb.linearVelocity = new Vector2(dashDir * dashVel, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        spriteRenderer.color = originalColor;
        isDashing = false;

        yield return new WaitForSeconds(dashInterval);
        canDashNext = true;
    }

    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer | platformLayer);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = isGrounded ? Color.green : Color.red;
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

    public void OnAttackEnd() => isAttacking = false;

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & platformLayer) != 0) currentPlatform = col.gameObject;
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (((1 << col.gameObject.layer) & platformLayer) != 0) currentPlatform = null;
    }
}