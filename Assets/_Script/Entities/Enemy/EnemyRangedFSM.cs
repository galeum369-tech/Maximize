using UnityEngine;

public class EnemyRangedFSM : MonoBehaviour
{
    private enum State { Idle, Chase, Attack, Flee }
    private State currentState;

    [Header("원거리 전투 설정")]
    public float attackRange = 7f;      // 총을 쏠 수 있는 사거리
    public float safeDistance = 4f;     // 이 거리보다 좁혀지면 뒤로 도망감
    public float attackCooldown = 2f;   // 공격 쿨타임

    [Header("발사체 설정")]
    public GameObject projectilePrefab; // 날아갈 총알 프리팹
    public Transform firePoint;         // 총알이 나갈 위치 (총구)

    private Transform player;
    private float lastAttackTime;

    private void Start()
    {
        currentState = State.Idle;
    }

    private void Update()
    {
        // [핵심 추가] 타겟 갱신 로직 (위와 동일)
        if (GameManager.Instance != null)
        {
            Transform activeTarget = GameManager.Instance.GetActivePlayer();

            // 내 타겟이 없거나, 꺼졌거나, 실제 플레이어랑 다르면 갱신
            if (player == null || !player.gameObject.activeInHierarchy || player != activeTarget)
            {
                player = activeTarget;
            }
        }

        // 플레이어 없으면 아무것도 안 함
        if (player == null) return;

        // 2. 플레이어와의 거리 계산
        float distance = Vector2.Distance(transform.position, player.position);

        // 3. 상태 머신 실행
        switch (currentState)
        {
            case State.Idle:
                // 사거리 + 조금 더 먼 거리 안에 들어오면 인식하고 쫓아감
                if (distance < attackRange + 3f)
                    currentState = State.Chase;
                break;

            case State.Chase:
                if (distance < safeDistance)
                {
                    // 너무 가까우면 도망
                    currentState = State.Flee;
                }
                else if (distance <= attackRange)
                {
                    // 사거리 안이면 공격 모세
                    currentState = State.Attack;
                }
                else
                {
                    // 아직 멀면 다가감
                    transform.position = Vector3.MoveTowards(transform.position, player.position, 2f * Time.deltaTime);
                }
                break;

            case State.Attack:
                if (distance > attackRange)
                {
                    // 멀어지면 다시 추적
                    currentState = State.Chase;
                    break;
                }
                else if (distance < safeDistance)
                {
                    // 때리다가 가까워지면 도망
                    currentState = State.Flee;
                    break;
                }

                // 쿨타임 돌면 총알 발사
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    ShootProjectile();
                    lastAttackTime = Time.time;
                }
                break;

            case State.Flee:
                if (distance > safeDistance + 1f)
                {
                    // 충분히 거리를 벌렸으면 다시 공격
                    currentState = State.Attack;
                }
                else
                {
                    // 플레이어의 반대 방향으로 도망 (카이팅)
                    Vector3 fleeDir = (transform.position - player.position).normalized;
                    transform.position = Vector3.MoveTowards(transform.position, transform.position + fleeDir, 3f * Time.deltaTime);
                }
                break;
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab != null && firePoint != null)
        {
            // 총알 생성
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // (참고) 총알 프리팹에 날아가는 스크립트가 있다면 방향을 넘겨줌
            Vector3 shootDir = (player.position - firePoint.position).normalized;

            // 예시: projectile.GetComponent<Projectile>().SetDirection(shootDir);
            Debug.Log("빵야!");
        }
    }

    // --- [추가된 기즈모 그리기 함수] ---
    private void OnDrawGizmosSelected()
    {
        // 에디터에서 해당 오브젝트를 선택했을 때만 범위가 보임

        // 1. 공격 가능 사거리 (빨간색 선)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 2. 도망치는 안전 거리 (파란색 선)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, safeDistance);
    }
}