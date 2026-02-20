using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ParabolicBomb : MonoBehaviour
{
    [Header("조준 및 충돌 설정")]
    public float detectRadius = 10f;
    public LayerMask targetLayer;
    public LayerMask obstacleLayer;
    public float timeToTarget = 0.8f;
    public float defaultDistance = 5f;
    public float maxLifetime = 5f; // 혹시 모를 상황 대비

    [Header("폭발 설정")]
    public GameObject explosionPrefab;

    private Rigidbody2D rb;
    private float ownerAtk;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // [추가] 5초 뒤 자동 삭제
        Destroy(gameObject, maxLifetime);
    }

    // ParabolicBomb.cs 수정 부분
    public void Launch(float atk, Vector2 forwardDir)
    {
        ownerAtk = atk;

        // 전방 120도 이내의 적 탐색
        Transform target = FindTargetInCone(forwardDir);

        Vector2 targetPos;
        if (target != null)
        {
            targetPos = (Vector2)target.position;
        }
        else
        {
            // 시야 내에 적이 없으면 바라보는 정면의 일정 거리 지점을 타겟으로 잡음
            targetPos = (Vector2)transform.position + (forwardDir * defaultDistance);
        }

        CalculateArc(targetPos);
    }

    private Transform FindTargetInCone(Vector2 forward)
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, detectRadius, targetLayer);
        Transform nearest = null;
        float minDst = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            Vector2 dirToEnemy = (enemy.transform.position - transform.position).normalized;

            // 정면 기준 좌우 60도(총 120도) 체크
            if (Vector2.Angle(forward, dirToEnemy) <= 60f)
            {
                float dst = Vector2.Distance(transform.position, enemy.transform.position);
                if (dst < minDst)
                {
                    minDst = dst;
                    nearest = enemy.transform;
                }
            }
        }
        return nearest;
    }

    private void CalculateArc(Vector2 targetPos)
    {
        Vector2 displacement = targetPos - rb.position;
        float velocityX = displacement.x / timeToTarget;
        float velocityY = (displacement.y + 0.5f * Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale * timeToTarget * timeToTarget) / timeToTarget;

        rb.linearVelocity = new Vector2(velocityX, velocityY);
        rb.angularVelocity = 360f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & (targetLayer | obstacleLayer)) != 0)
        {
            Explode();
        }
    }

    private void Explode()
    {
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            UniversalHitbox hb = exp.GetComponent<UniversalHitbox>();
            if (hb != null) hb.SetOwnerAtk(ownerAtk);
        }

        // [수정] SetActive(false) 대신 Destroy 사용
        Destroy(gameObject);
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