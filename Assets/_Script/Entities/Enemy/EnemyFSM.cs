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
        if (owner.player == null) return;

        float dist = Vector2.Distance(transform.position, owner.player.position);

        // owner.currentState와 이 스크립트의 currentState를 동기화 (선택 사항)
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
                else owner.Attack(); // 이제 EnemyBase에 Attack이 있음
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