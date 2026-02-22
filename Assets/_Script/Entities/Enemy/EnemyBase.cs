using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("데이터 참조")]
    public EnemyData data;

    // --- 실시간 결정된 스탯 ---
    protected float finalMaxHp;
    public float currentHp;
    protected float finalDamage;
    public float FinalDamage => finalDamage;
    protected float finalDefense;
    protected float finalMoveSpeed;

    // --- FSM 상태 (FSM 스크립트에서 제어하도록 public으로 변경) ---
    public EnemyState currentState = EnemyState.Idle;
    protected float attackTimer = 0f;

    // 컨트롤러들
    protected EnemyMoveController mc;
    protected EnemyAnimController ac;
    [HideInInspector] public Transform player; // FSM에서 접근 가능하도록 public
    protected Rigidbody2D rb;

    // 사망 시 알림을 위한 이벤트 추가
    public System.Action<EnemyBase> OnDeathEvent;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mc = new EnemyMoveController(rb, transform);
        ac = new EnemyAnimController(GetComponent<Animator>());

        if (data != null) InitializeStats();
    }

    protected virtual void InitializeStats()
    {
        // 1. 맵 타입에 따른 기본 배율 결정 (필드 vs 던전)
        float areaMultiplier = 1.0f;
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentState == GameState.Field)
                areaMultiplier = 0.7f;  // 필드는 30% 약하게 시작
            else if (GameManager.Instance.currentState == GameState.Dungeon)
                areaMultiplier = 1.2f;  // 던전은 20% 강하게 시작
        }

        // 2. 개체별 랜덤 변동치 적용 (유저가 작성한 로직)
        // 맵 배율에 랜덤 변동치를 곱해서 최종 배율을 만듦
        float individualMultiplier = Random.Range(0.75f, 1.25f);
        float finalMultiplier = areaMultiplier * individualMultiplier;

        // 3. 최종 스탯 계산
        finalMaxHp = data.baseMaxHp * finalMultiplier;
        finalDamage = data.baseDamage * finalMultiplier;
        finalDefense = data.baseDefense * finalMultiplier;

        // 이동속도는 맵 배율보다는 개체별 랜덤성만 주는 게 자연스러워 (너무 빠르면 대응이 힘드니까)
        finalMoveSpeed = data.baseMoveSpeed * Random.Range(0.9f, 1.1f);

        // 현재 체력 풀로 채우기
        currentHp = finalMaxHp;

        // 디버그 찍어보면 밸런스 잡기 편함
        // Debug.Log($"{gameObject.name} 생성! 배율: {finalMultiplier:F2} (지역:{areaMultiplier} * 개체:{individualMultiplier:F2})");
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // --- 외부(FSM)에서 호출하는 메서드들 ---

    public virtual void MoveToPlayer()
    {
        if (player == null) return;
        ac.PlayMove(1f); // float 인자 전달
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        mc.Move(direction, finalMoveSpeed); // mc.Move로 이름 통일
    }

    public virtual void StopMove()
    {
        ac.PlayMove(0f); // 멈춤 애니메이션
        mc.Stop();
    }

    public virtual void Attack()
    {
        StopMove();
        attackTimer += Time.deltaTime;
        // 공격 속도는 data에서 가져오거나 고정값 사용
        if (attackTimer >= 1.5f)
        {
            ac.PlayAttack(); // 인자 없는 버전으로 호출
            attackTimer = 0f;
        }
    }

    public virtual void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
        if (currentState == EnemyState.Die) return;

        float reduction = finalDefense / 100f;
        float actualDamage = Mathf.Max(damage * (1f - reduction), 1f);

        currentHp -= actualDamage;
        ac.PlayHit();

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(hitDirection * knockback, ForceMode2D.Impulse);

        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        if (currentState == EnemyState.Die) return;
        currentState = EnemyState.Die;

        ac.PlayDeath();

        // [핵심 추가] 죽을 때 아이템을 바닥에 뿌림!
        DropItems();

        // 사망 이벤트 호출 (MapPiece가 방 클리어 체크할 때 씀)
        OnDeathEvent?.Invoke(this);

        this.enabled = false;

        Destroy(gameObject, 2f);
    }

    protected void DropItems()
    {
        if (data.lootTable == null) return;
        foreach (var loot in data.lootTable)
        {
            if (Random.Range(0f, 100f) <= loot.dropChance)
            {
                GameObject dropped = Instantiate(loot.itemPrefab, transform.position, Quaternion.identity);
                Rigidbody2D itemRb = dropped.GetComponent<Rigidbody2D>();
                if (itemRb != null)
                {
                    itemRb.AddForce(new Vector2(Random.Range(-1f, 1f), 2f), ForceMode2D.Impulse);
                }
            }
        }
    }
}