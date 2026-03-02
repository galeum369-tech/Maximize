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
        // [핵심 추가] 현재 타겟이 꺼졌거나(변신해서), 타겟이 바뀌었는지 확인
        Transform activePlayer = GameManager.Instance.GetActivePlayer();

        // 1. 타겟 갱신 로직
        if (owner.player == null || !owner.player.gameObject.activeInHierarchy || owner.player != activePlayer)
        {
            owner.player = activePlayer; // 강제로 최신 플레이어(메카/휴먼)로 교체

            // 만약 그래도 없으면 리턴
            if (owner.player == null) return;
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