using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DroneCannon : MonoBehaviour
{
    [Header("캐논 설정")]
    public float speed = 15f;
    public float maxLifetime = 3f;
    public LayerMask contactLayers;

    [Header("폭발 설정")]
    public GameObject explosionPrefab;

    private Rigidbody2D rb;
    private float cannonDamage;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

            // [중요] 새로 소환된 폭발 히트박스에 공격력 주입
            if (hb != null)
            {
                hb.SetOwnerAtk(cannonDamage);
            }
        }

        Destroy(gameObject);
    }
}