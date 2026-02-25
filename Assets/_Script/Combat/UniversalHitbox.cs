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
    public bool canHitMultiple = true;

    public System.Action<Vector2> OnHitEffect;

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private Collider2D myCollider;
    private float ownerAtk = 0f;
    private bool isInitialized = false;

    private PlayerState pState;
    private MechaState mState;
    private EnemyBase eBase;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        pState = GetComponentInParent<PlayerState>();
        mState = GetComponentInParent<MechaState>();
        eBase = GetComponentInParent<EnemyBase>();

        RefreshOwnerAtk();
    }

    private void OnEnable()
    {
        ResetHitbox();
        RefreshOwnerAtk();

        if (isInitialized)
        {
            CheckImmediateOverlap();
        }
    }

    public void RefreshOwnerAtk()
    {
        if (mState != null) { ownerAtk = mState.FinalAtk; isInitialized = true; }
        else if (pState != null) { ownerAtk = pState.FinalAtk; isInitialized = true; }
        else if (eBase != null) { ownerAtk = eBase.FinalDamage; isInitialized = true; }
    }

    public void SetOwnerAtk(float atk)
    {
        ownerAtk = atk;
        isInitialized = true;
        ResetHitbox();
        CheckImmediateOverlap();
    }

    public void ResetHitbox()
    {
        hitTargets.Clear();
    }

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

            Vector2 hitPoint = collision.ClosestPoint(transform.position);
            if (Vector2.Distance(hitPoint, transform.position) < 0.05f)
            {
                hitPoint = collision.transform.position;
            }

            float finalDamage = ownerAtk * damageMultiplier;
            Vector2 hitDir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackPower, hitDir);
            OnHitEffect?.Invoke(hitPoint);

            // ==========================================
            // [핵심 수정] 부모(pState) 체크 대신 전역 스킬 컨트롤러를 확인
            // ==========================================
            if (PlayerSkillController.Instance != null && !PlayerSkillController.Instance.isMechaForm)
            {
                PlayerSkillController.Instance.AddMechaEnergy(); // 인간 폼이면 투사체 타격도 게이지 증가!
            }
        }
    }

    public void DestroySelf() => Destroy(gameObject);
}