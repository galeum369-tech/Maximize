using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DroneCannon : MonoBehaviour
{
    [Header("캐논 설정")]
    public float speed = 15f;
    public float maxLifetime = 3f; // 이 시간이 지나면 자동 삭제
    public LayerMask contactLayers;

    [Header("폭발 설정")]
    public GameObject explosionPrefab;

    private Rigidbody2D rb;
    private float cannonDamage;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // [보완] 생성되자마자 타이머를 걸어 메모리 누수 방지
        Destroy(gameObject, maxLifetime);
    }

    public void Launch(float dmg, Vector2 direction)
    {
        cannonDamage = dmg;
        rb.linearVelocity = direction.normalized * speed;
        transform.right = direction.normalized;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit) return;

        if (((1 << collision.gameObject.layer) & contactLayers) != 0)
        {
            hasHit = true;
            HitTarget(collision.transform.position);
        }
    }

    private void HitTarget(Vector2 hitPos)
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, hitPos, Quaternion.identity);
            UniversalHitbox hb = exp.GetComponent<UniversalHitbox>();
            if (hb != null) hb.SetOwnerAtk(cannonDamage);
        }

        // [수정] 즉시 삭제
        Destroy(gameObject);
    }
}