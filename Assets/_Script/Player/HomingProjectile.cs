using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("이동 설정")]
    public float speed = 12f;
    public float rotateSpeed = 600f;
    public float detectRadius = 8f;
    public LayerMask targetLayer;
    public float maxLifetime = 4f; // 유도 실패 시 자동 삭제

    [Header("데미지 설정")]
    public float damageMultiplier = 1f;
    public float knockbackPower = 5f;

    private Transform target;
    private Rigidbody2D rb;
    private float ownerAtk;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // [추가] 유도탄이 맵 밖으로 무한히 나가는 걸 방지
        Destroy(gameObject, maxLifetime);
    }

    public void Launch(float atk)
    {
        ownerAtk = atk;
        target = FindNearestTarget();
    }

    private void FixedUpdate()
    {
        if (target != null && target.gameObject.activeInHierarchy)
        {
            Vector2 direction = (Vector2)target.position - rb.position;
            direction.Normalize();
            float rotateAmount = Vector3.Cross(transform.right, direction).z;
            rb.angularVelocity = rotateAmount * rotateSpeed;
        }
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayer) != 0)
        {
            IDamageable damageable = collision.GetComponent<IDamageable>();
            if (damageable != null)
            {
                float finalDamage = ownerAtk * damageMultiplier;
                Vector2 hitDir = (collision.transform.position - transform.position).normalized;
                damageable.TakeDamage(finalDamage, knockbackPower, hitDir);
            }

            // [수정] 비활성화 대신 파괴
            Destroy(gameObject);
        }
    }

    private Transform FindNearestTarget()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectRadius, targetLayer);
        Transform nearest = null;
        float minDst = Mathf.Infinity;
        foreach (var enemy in enemies)
        {
            float dst = Vector2.Distance(transform.position, enemy.transform.position);
            if (dst < minDst) { minDst = dst; nearest = enemy.transform; }
        }
        return nearest;
    }
}