using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(MechaState))]
[RequireComponent(typeof(CapsuleCollider2D))] // 콜라이더 필수
public class Mech : MonoBehaviour, IDamageable
{
    // --- 모듈 클래스 및 컴포넌트 ---
    private MechMoveController mc;
    private MechAnimController ac;
    private Rigidbody2D rb;
    private Animator anim;
    private PlayerInputHandler input;
    public MechaState state;
    private CapsuleCollider2D mechaCollider; // [추가] 본인 콜라이더 참조

    [Header("발사 위치 및 투사체")]
    public Transform[] firePoints;
    public GameObject normalBulletPrefab;
    public GameObject bombPrefab;
    public GameObject[] laserObjects;

    [Header("이동 설정")]
    public float jumpForce = 14f;
    public float hoverGravity = -2f;
    [Range(0f, 1f)]
    public float shootMovePenalty = 0.75f;

    [Header("지면 및 플랫폼 체크")] // [수정] 헤더 변경
    public Transform groundCheck;
    public LayerMask groundLayer;
    public LayerMask platformLayer; // [추가] 플랫폼 레이어 (Inspector에서 설정)
    public float groundRadius = 0.2f;

    // --- 상태 변수 ---
    private bool isGrounded;
    private bool isGliding;
    private bool isSkillActive;

    // [추가] 하향 점프용 변수
    private GameObject currentPlatform;

    // 입력 값 저장
    private Vector2 moveInput;
    private bool isJumpHeld;
    private bool isAttackHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        input = GetComponent<PlayerInputHandler>();
        state = GetComponent<MechaState>();
        mechaCollider = GetComponent<CapsuleCollider2D>(); // [추가]

        mc = new MechMoveController(rb);
        ac = new MechAnimController(anim);

        if (laserObjects != null)
        {
            foreach (var laser in laserObjects) if (laser) laser.SetActive(false);
        }
    }

    private void OnEnable()
    {
        input.OnMove += (vec) => moveInput = vec;
        input.OnJump += HandleJump; // [수정] 메서드로 분리 (복잡해져서)
        input.OnAttack += (pressed) => isAttackHeld = pressed;
        input.OnSkill1 += UseSkillA;
        input.OnSkill2 += UseSkillS;
    }

    private void OnDisable()
    {
        input.OnMove -= (vec) => moveInput = vec;
        input.OnJump -= HandleJump;
        input.OnAttack -= (pressed) => isAttackHeld = pressed;
        input.OnSkill1 -= UseSkillA;
        input.OnSkill2 -= UseSkillS;

        isSkillActive = false;
        isJumpHeld = false;
        isAttackHeld = false;
        moveInput = Vector2.zero;
    }

    private void Update()
    {
        // 1. 지면 체크 (Ground + Platform 둘 다 바닥으로 인식)
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer | platformLayer);

        // 2. 활공 판단
        isGliding = !isGrounded && rb.linearVelocity.y < -0.1f && isJumpHeld && !isSkillActive;

        // 3. 애니메이션 상태 업데이트
        ac.UpdateAerialState(isGrounded, rb.linearVelocity.y, isGliding);

        // 4. 애니메이션 분기 처리
        HandleActionAnimation();
    }

    private void FixedUpdate()
    {
        if (isSkillActive) { mc.Stop(); return; }

        // 이동 로직
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            float targetSpeed = state.FinalSpd;
            if (isAttackHeld) targetSpeed *= shootMovePenalty;

            mc.Move(moveInput.x, targetSpeed);
            FlipSprite(moveInput.x);
        }
        else
        {
            mc.Stop();
        }

        // 활공 물리 적용
        if (isGliding) mc.Glide();
    }

    // --- [핵심 수정] 점프 핸들러 ---
    private void HandleJump(bool pressed)
    {
        isJumpHeld = pressed;
        if (!pressed) return; // 버튼 뗄 때는 무시

        // 1. 하향 점프 (아래 키 + 점프 + 플랫폼 위에 있음) [추가]
        if (moveInput.y < -0.5f && currentPlatform != null)
        {
            StartCoroutine(DownJump());
        }
        // 2. 일반 점프 (바닥에 있을 때)
        else if (isGrounded)
        {
            mc.Jump(jumpForce);
        }
    }

    // --- [추가] 하향 점프 코루틴 ---
    private IEnumerator DownJump()
    {
        Collider2D platformCol = currentPlatform.GetComponent<Collider2D>();

        if (platformCol != null && mechaCollider != null)
        {
            // 0.5초 동안 플레이어와 발판의 충돌을 무시
            Physics2D.IgnoreCollision(mechaCollider, platformCol, true);
            yield return new WaitForSeconds(0.5f);
            Physics2D.IgnoreCollision(mechaCollider, platformCol, false);
        }
    }

    // --- [추가] 플랫폼 감지 (충돌 이벤트) ---
    private void OnCollisionEnter2D(Collision2D col)
    {
        // 부딪힌 물체가 Platform Layer라면 저장
        if (((1 << col.gameObject.layer) & platformLayer) != 0)
        {
            currentPlatform = col.gameObject;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        // 발판에서 벗어나면 초기화
        if (((1 << col.gameObject.layer) & platformLayer) != 0)
        {
            currentPlatform = null;
        }
    }

    // --- 이하 기존 로직 동일 ---
    private void HandleActionAnimation()
    {
        if (isSkillActive) return;

        // 지면에 있을 때만 걷기/사격 애니메이션 처리 (공중에서는 AerialState가 우선)
        if (!isGrounded) return;

        bool isMoving = Mathf.Abs(moveInput.x) > 0.01f;

        // 1. 이동 사격 (Move=true, MoveAttack=true)
        if (isMoving && isAttackHeld)
        {
            ac.PlayMoveAttack(true, true);
            ac.PlayAttack(false); // 제자리 사격 파라미터는 끔
        }
        // 2. 제자리 사격 (Move=false, Attack=true)
        else if (!isMoving && isAttackHeld)
        {
            ac.PlayAttack(true);
            ac.PlayMoveAttack(false, false);
        }
        // 3. 일반 이동 또는 대기 (Move=isMoving, 그 외 false)
        else
        {
            ac.PlayMove(isMoving);
            ac.PlayAttack(false);
            ac.PlayMoveAttack(isMoving, false);
        }
    }

    private void FlipSprite(float xDir)
    {
        float absX = Mathf.Abs(transform.localScale.x);
        transform.localScale = new Vector3(Mathf.Sign(xDir) * absX, transform.localScale.y, 1);
    }

    // Mech.cs의 스킬 실행 함수 예시
    private void UseSkillA()
    {
        if (isSkillActive || !isGrounded) return;
        isSkillActive = true;

        // 왼쪽을 보고 있다면 애니메이션 재생 속도를 -1로 하거나, 
        // 레이저 오브젝트의 로컬 Y축을 180도 돌려버리는 꼼수가 있어.
        if (transform.localScale.x < 0)
        {
            // 왼쪽일 때만 레이저를 담은 부모의 Y를 180도 돌려 거울 반전을 상쇄
            foreach (var l in laserObjects) l.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }
        else
        {
            foreach (var l in laserObjects) l.transform.localRotation = Quaternion.identity;
        }

        mc.Stop();
        ac.PlaySkillA();
    }
    private void UseSkillS() { if (isSkillActive || !isGrounded) return; isSkillActive = true; mc.Stop(); ac.PlaySkillS(); }

    // 이벤트 메서드들 (기존과 동일)
    public void OnFireNormal() { if (normalBulletPrefab) FireAllPoints(normalBulletPrefab); }
    public void OnFireBomb() { if (bombPrefab) FireAllPoints(bombPrefab); }
    public void OnLaserOn()
    {
        if (laserObjects == null) return;
        foreach (var l in laserObjects)
        {
            if (l == null) continue;
            var laserScript = l.GetComponent<MechaLaser>();
            if (laserScript) laserScript.Activate(state.FinalAtk);
        }
    }
    // Mech.cs 내부

    // 스킬 애니메이션이 끝나거나 강제로 종료될 때 호출되는 함수
    public void OnSkillEnd()
    {
        isSkillActive = false;

        // --- [핵심 수정 2] 레이저 강제 종료 ---
        // 애니메이션 이벤트가 혹시 씹히더라도 스킬 상태가 끝나면 무조건 레이저를 끕니다.
        OnLaserOff();
    }

    // 기존 메서드 (수정 없음)
    public void OnLaserOff()
    {
        if (laserObjects == null) return;
        foreach (var l in laserObjects)
        {
            if (l == null) continue;
            var laserScript = l.GetComponent<MechaLaser>();
            if (laserScript) laserScript.Deactivate();
        }
    }

    public void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
        state.TakeDamage(damage);

        // 피격 애니메이션 재생 추가!
        if (state.currentHp > 0)
        {
            ac.PlayHit();
        }

        rb.AddForce(hitDirection * (knockback * 0.3f), ForceMode2D.Impulse);

        if (state.currentHp <= 0)
        {
            ac.PlayDeath();
        }
    }

    private void FireAllPoints(GameObject prefab)
    {
        // 1. 현재 메카닉이 보고 있는 방향 확정 (1: 오른쪽, -1: 왼쪽)
        float facing = Mathf.Sign(transform.localScale.x);

        foreach (Transform fp in firePoints)
        {
            if (fp == null) continue;

            // 투사체 생성
            GameObject proj = Instantiate(prefab, fp.position, fp.rotation);

            // 2. [핵심] fp.right를 그대로 쓰지 않고, 메카닉의 facing을 기준으로 방향 벡터 생성
            // fp의 y축 회전(기울기)은 유지하면서 x축 방향만 메카닉 방향으로 강제 고정
            Vector2 forcedDir = fp.right;

            // 혹시 fp.right가 부모 스케일 때문에 꼬였다면 아래처럼 강제로 방향을 잡아줌
            if (facing > 0 && forcedDir.x < 0) forcedDir.x *= -1;
            else if (facing < 0 && forcedDir.x > 0) forcedDir.x *= -1;

            // 유도탄에 방향 전달
            var homing = proj.GetComponent<HomingProjectile>();
            if (homing != null) homing.Launch(state.FinalAtk, forcedDir);

            // 폭탄에 방향 전달
            var bomb = proj.GetComponent<ParabolicBomb>();
            if (bomb != null) bomb.Launch(state.FinalAtk, forcedDir);
        }
    }
}