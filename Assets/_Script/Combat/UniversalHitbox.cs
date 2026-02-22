using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider2D))]
public class UniversalHitbox : MonoBehaviour
{
    [Header("타격 설정")]
    public LayerMask targetLayers;
    public float damageMultiplier = 1f;
    public float knockbackPower = 10f;

    [Header("옵션")]
    public bool canHitMultiple = true; // true면 여러 대상을 동시에 타격, false면 첫 대상만 타격

    // 외부에서 타격 이펙트를 재생할 수 있도록 이벤트 추가
    public System.Action<Vector2> OnHitEffect;

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private Collider2D myCollider;
    private float ownerAtk = 0f;
    private bool isInitialized = false;

    // 참조용 State들
    private PlayerState pState;
    private MechaState mState;
    private EnemyBase eBase;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();

        // 부모로부터 모든 가능한 State 탐색
        pState = GetComponentInParent<PlayerState>();
        mState = GetComponentInParent<MechaState>();
        eBase = GetComponentInParent<EnemyBase>();

        RefreshOwnerAtk();
    }

    private void OnEnable()
    {
        ResetHitbox(); // 켜질 때 타겟 목록 초기화
        RefreshOwnerAtk();

        if (isInitialized)
        {
            CheckImmediateOverlap();
        }
    }

    // 부모 타입에 따라 최신 공격력을 가져옴
    public void RefreshOwnerAtk()
    {
        if (mState != null)
        {
            ownerAtk = mState.FinalAtk;
            isInitialized = true;
        }
        else if (pState != null)
        {
            ownerAtk = pState.FinalAtk;
            isInitialized = true;
        }
        else if (eBase != null)
        {
            ownerAtk = eBase.FinalDamage;
            isInitialized = true;
        }
    }

    // 외부 주입용 (폭탄, 드론 등 부모가 없는 경우)
    public void SetOwnerAtk(float atk)
    {
        ownerAtk = atk;
        isInitialized = true;
        ResetHitbox();
        CheckImmediateOverlap();
    }

    // [추가] 다단 히트를 위해 타격 목록을 비우는 기능
    // 레이저 같은 다단 히트 무기는 0.1초마다 이 함수를 호출하면 됨
    public void ResetHitbox()
    {
        hitTargets.Clear();
    }

    // 즉시 겹쳐 있는 적 판정 (애니메이션 첫 프레임용)
    private void CheckImmediateOverlap()
    {
        if (myCollider == null || !myCollider.enabled || !isInitialized) return;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();
        int hitCount = myCollider.Overlap(filter, results);

        for (int i = 0; i < hitCount; i++) ProcessHit(results[i]);
    }

    private void OnTriggerEnter2D(Collider2D collision) => ProcessHit(collision);

    private void ProcessHit(Collider2D collision)
    {
        if (!isInitialized || ownerAtk <= 0) return;
        if (((1 << collision.gameObject.layer) & targetLayers) == 0) return;
        if (hitTargets.Contains(collision.gameObject)) return;
        if (!canHitMultiple && hitTargets.Count > 0) return;

        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            hitTargets.Add(collision.gameObject);

            // --- [핵심 수정: 타격 지점 계산] ---
            // 1. 내 위치에서 가장 가까운 상대방 콜라이더의 지점을 찾음
            // 보스처럼 큰 몬스터라도 히트박스와 맞닿은 표면 좌표를 가져옴
            Vector2 hitPoint = collision.ClosestPoint(transform.position);

            // 만약 ClosestPoint가 내 위치와 너무 가깝다면(이미 겹친 경우), 
            // 그냥 적의 중앙 좌표를 대안으로 사용
            if (Vector2.Distance(hitPoint, transform.position) < 0.05f)
            {
                hitPoint = collision.transform.position;
            }

            float finalDamage = ownerAtk * damageMultiplier;
            Vector2 hitDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackPower, hitDir);

            // 2. 계산된 hitPoint를 이벤트로 전달!
            OnHitEffect?.Invoke(hitPoint);

            Debug.Log($"<color=white>[Hit]</color> {collision.gameObject.name}의 {hitPoint} 지점 타격!");
        }
    }

    public void DestroySelf() => Destroy(gameObject);
}