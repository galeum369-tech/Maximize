using UnityEngine;
using System.Collections;

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
    }

    private void Start()
    {
        // 내장된 Destroy 타이머 대신, 코루틴으로 수명 관리 시작
        StartCoroutine(LifetimeRoutine());
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
            // 적이나 벽에 맞았을 때는 맞은 정확한 위치(ClosestPoint)에서 터짐
            HitTarget(collision.ClosestPoint(transform.position));
        }
    }

    // 허공에서 수명이 다했을 때의 처리
    private IEnumerator LifetimeRoutine()
    {
        yield return new WaitForSeconds(maxLifetime);

        if (!hasHit)
        {
            hasHit = true;
            // 허공에서 시간이 다 됐으면 '현재 총알이 있는 위치'에서 터지게 강제함
            HitTarget(transform.position);
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

        Destroy(gameObject);
    }
}