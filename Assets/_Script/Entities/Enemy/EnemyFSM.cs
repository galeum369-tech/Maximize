using UnityEngine;

// enum을 여기서 한 번만 정의 (다른 파일에서는 삭제)
public enum EnemyState { Idle, Chase, Attack, Hit, Die }

public class EnemyFSM : MonoBehaviour
{
    private EnemyBase owner;

    public EnemyState currentState = EnemyState.Idle;

    [Header("인식 설정")]
    public float detectRange = 8f;
    public float attackRange = 1.5f;

    private void Awake()
    {
        owner = GetComponent<EnemyBase>();
    }

    private void Start()
    {
        currentState = EnemyState.Idle;
    }

    private void Update()
    {
        // [핵심 추가] EnemyBase가 체력 등의 세팅을 완전히 끝내기 전까진 FSM 작동 중지!
        if (!owner.isInitialized) return;

        if (owner.currentHp <= 0)
        {
            currentState = EnemyState.Die;
            return;
        }

        UpdateState();
    }

    private void UpdateState()
    {
        if (owner.player == null)
        {
            if (GameManager.Instance != null && GameManager.Instance.GetActivePlayer() != null)
            {
                owner.player = GameManager.Instance.GetActivePlayer();
            }
            else
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null) owner.player = pObj.transform;
            }

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