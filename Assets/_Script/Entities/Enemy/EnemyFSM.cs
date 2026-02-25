using UnityEngine;

// enum을 여기서 한 번만 정의 (다른 파일에서는 삭제)
public enum EnemyState { Idle, Chase, Attack, Hit, Die }

public class EnemyFSM : MonoBehaviour
{
    private EnemyBase owner;

    // 인스펙터에서 실수로 Die로 저장했더라도 무시되도록 함
    public EnemyState currentState = EnemyState.Idle;

    [Header("인식 설정")]
    public float detectRange = 8f;
    public float attackRange = 1.5f;

    private void Awake()
    {
        owner = GetComponent<EnemyBase>();
    }

    // ==========================================
    // [추가] 시작할 때 무조건 Idle로 강제 초기화!
    // ==========================================
    private void Start()
    {
        currentState = EnemyState.Idle;
    }

    private void Update()
    {
        if (owner.currentHp <= 0)
        {
            currentState = EnemyState.Die;
            return;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        // ==========================================
        // [수정] 플레이어가 없으면 지속적으로 다시 찾음!
        // ==========================================
        if (owner.player == null)
        {
            // GameManager를 통해 확실하게 살아있는 폼(인간/메카)을 찾음
            if (GameManager.Instance != null && GameManager.Instance.GetActivePlayer() != null)
            {
                owner.player = GameManager.Instance.GetActivePlayer();
            }
            else // 보험용 태그 검색
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null) owner.player = pObj.transform;
            }

            // 그래도 못 찾았으면 가만히 대기
            if (owner.player == null)
            {
                currentState = EnemyState.Idle;
                owner.currentState = currentState;
                owner.StopMove();
                return;
            }
        }

        float dist = Vector2.Distance(transform.position, owner.player.position);

        owner.currentState = currentState;

        switch (currentState)
        {
            case EnemyState.Idle:
                if (dist < detectRange) currentState = EnemyState.Chase;
                owner.StopMove();
                break;

            case EnemyState.Chase:
                if (dist <= attackRange) currentState = EnemyState.Attack;
                else if (dist > detectRange) currentState = EnemyState.Idle;
                else owner.MoveToPlayer();
                break;

            case EnemyState.Attack:
                if (dist > attackRange) currentState = EnemyState.Chase;
                else owner.Attack();
                break;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(gameObject.transform.position, detectRange);
    }
}