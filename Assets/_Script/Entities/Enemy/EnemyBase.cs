using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("데이터 참조")]
    public EnemyData data;

    protected float finalMaxHp;
    public float currentHp;
    protected float finalDamage;
    public float FinalDamage => finalDamage;
    protected float finalDefense;
    protected float finalMoveSpeed;

    // 여기도 기본값을 Idle로 꽉 잡아줌
    public EnemyState currentState = EnemyState.Idle;
    protected float attackTimer = 0f;

    protected EnemyMoveController mc;
    protected EnemyAnimController ac;
    [HideInInspector] public Transform player;
    protected Rigidbody2D rb;

    public System.Action<EnemyBase> OnDeathEvent;

    // [핵심 추가] 초기화 완료 여부를 FSM에 알려줄 플래그
    [HideInInspector] public bool isInitialized = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mc = new EnemyMoveController(rb, transform);
        ac = new EnemyAnimController(GetComponent<Animator>());
    }

    // [핵심 추가] MapPiece 같은 외부 스크립트에서 생성 즉시 호출할 수 있는 셋업 함수
    public virtual void Setup(EnemyData newData)
    {
        this.data = newData;
        InitializeStats();
        currentState = EnemyState.Idle;
        player = GameManager.Instance?.GetActivePlayer();
        isInitialized = true; // 세팅 완료!
    }

    protected virtual void Start()
    {
        // Setup() 함수로 미리 초기화되지 않았을 때만 (ex. 마을에 미리 배치해둔 적) Start에서 초기화 진행
        if (!isInitialized)
        {
            if (data != null) InitializeStats();
            currentState = EnemyState.Idle;
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            isInitialized = true;
        }
    }

    protected virtual void InitializeStats()
    {
        float areaMultiplier = 1.0f;
        if (GameManager.Instance != null)
        {
            if (GameManager.Instance.currentState == GameState.Field)
                areaMultiplier = 0.7f;
            else if (GameManager.Instance.currentState == GameState.Dungeon)
                areaMultiplier = 1.2f;
        }

        float individualMultiplier = Random.Range(0.75f, 1.25f);
        float finalMultiplier = areaMultiplier * individualMultiplier;

        finalMaxHp = data.baseMaxHp * finalMultiplier;
        finalDamage = data.baseDamage * finalMultiplier;
        finalDefense = data.baseDefense * finalMultiplier;
        finalMoveSpeed = data.baseMoveSpeed * Random.Range(0.9f, 1.1f);

        currentHp = finalMaxHp;
    }

    public virtual void MoveToPlayer()
    {
        if (player == null) return;
        ac.PlayMove(1f);
        float direction = (player.position.x > transform.position.x) ? 1f : -1f;
        mc.Move(direction, finalMoveSpeed);
    }

    public virtual void StopMove()
    {
        ac.PlayMove(0f);
        mc.Stop();
    }

    public virtual void Attack()
    {
        StopMove();
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1.5f)
        {
            ac.PlayAttack();
            attackTimer = 0f;
        }
    }

    public virtual void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
        if (currentState == EnemyState.Die) return;

        float reduction = finalDefense / 100f;
        float actualDamage = Mathf.Max(damage * (1f - reduction), 1f);

        currentHp -= actualDamage;

        if (GameManager.Instance != null && GameManager.Instance.damageTextPrefab != null)
        {
            GameObject textObj = Instantiate(GameManager.Instance.damageTextPrefab, transform.position + Vector3.up, Quaternion.identity);
            textObj.GetComponent<DamageText>().Setup(actualDamage, false);
        }

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
        DropItems();
        OnDeathEvent?.Invoke(this);
        this.enabled = false;
        Destroy(gameObject, 2f);
    }

    protected void DropItems()
    {
        if (data == null || data.lootTable == null) return;
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