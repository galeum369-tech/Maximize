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
    public bool isProjectile = false;
    public bool canHitMultiple = true;

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private Collider2D myCollider;
    private PlayerState ownerState;

    public System.Action<Vector2> OnHitEffect;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();
        ownerState = transform.root.GetComponent<PlayerState>();
    }

    private void OnEnable()
    {
        hitTargets.Clear();
        CheckImmediateOverlap();
    }

    private void CheckImmediateOverlap()
    {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(targetLayers);
        filter.useTriggers = true;

        List<Collider2D> results = new List<Collider2D>();

        // [해결] OverlapCollider -> Overlap으로 변경 [cite: 2026-02-12]
        // 이 함수는 겹친 개수를 반환하고, results 리스트를 채워줘.
        int hitCount = myCollider.Overlap(filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            ProcessHit(results[i]);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessHit(collision);
    }

    private void ProcessHit(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & targetLayers) == 0) return;
        if (hitTargets.Contains(collision.gameObject)) return;
        if (!canHitMultiple && hitTargets.Count > 0) return;

        // PlayerState가 있으면 최종 스탯 반영, 없으면 기본 데미지(예: 10) 적용 [cite: 2026-02-12]
        float baseAtk = (ownerState != null) ? ownerState.FinalAtk : 10f;
        float finalDamage = baseAtk * damageMultiplier;

        IDamageable target = collision.GetComponent<IDamageable>();
        if (target != null)
        {
            hitTargets.Add(collision.gameObject);
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;

            target.TakeDamage(finalDamage, knockbackPower, hitDir);
            OnHitEffect?.Invoke(collision.transform.position);

            if (isProjectile) gameObject.SetActive(false);
        }
    }
}