using UnityEngine;

public class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("데이터 참조")]
    public EnemyData data; // 인스펙터에서 할당

    // --- 실시간 결정된 스탯 ---
    protected float finalMaxHp;
    public float currentHp;
    protected float finalDamage;
    public float FinalDamage => finalDamage;
    protected float finalDefense;
    protected float finalMoveSpeed;

    // 컨트롤러들
    protected EnemyMoveController mc;
    protected EnemyAnimController ac;
    protected Transform player;
    protected Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mc = new EnemyMoveController(rb, transform);
        ac = new EnemyAnimController(GetComponent<Animator>());

        if (data != null) InitializeStats();
    }

    // 스탯 랜덤 초기화 로직
    protected virtual void InitializeStats()
    {
        // 0.75 ~ 1.25 사이의 랜덤 계수 생성
        float multiplier = Random.Range(0.75f, 1.25f);

        // 계수 적용 (방어력이나 속도는 너무 널뛰지 않게 조절 가능)
        finalMaxHp = data.baseMaxHp * multiplier;
        finalDamage = data.baseDamage * multiplier;
        finalDefense = data.baseDefense * multiplier;
        finalMoveSpeed = data.baseMoveSpeed * Random.Range(0.9f, 1.1f); // 속도는 약간만 랜덤하게

        currentHp = finalMaxHp;

        Debug.Log($"{data.monsterName} 생성! HP: {finalMaxHp:F1}, ATK: {finalDamage:F1}");
    }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    public virtual void TakeDamage(float damage, float knockback, Vector2 hitDirection)
    {
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
        ac.PlayDeath();
        // 돈 드랍 예시
        int reward = Random.Range(data.dropMoneyMin, data.dropMoneyMax);
        // DataManager.Instance.AddGold(reward); 

        this.enabled = false;
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 2f);
    }
}