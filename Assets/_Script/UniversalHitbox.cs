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

    private HashSet<GameObject> hitTargets = new HashSet<GameObject>();
    private Collider2D myCollider;
    private float ownerAtk = 0f;
    private bool isInitialized = false; // 공격력 주입/탐색 완료 여부

    public System.Action<Vector2> OnHitEffect;

    private void Awake()
    {
        myCollider = GetComponent<Collider2D>();

        // 1. 부모로부터 스탯 자동 탐색 (근접 무기 등 자식 오브젝트용)
        // transform.root 대신 GetComponentInParent를 써서 Player/Mech 오브젝트의 스탯을 찾음
        var pState = GetComponentInParent<PlayerState>();
        var mState = GetComponentInParent<MechaState>();

        if (mState != null)
        {
            ownerAtk = mState.FinalAtk;
            isInitialized = true;
            Debug.Log($"<color=cyan>[Hitbox]</color> {gameObject.name}: 메카닉 스탯 확인 (Atk: {ownerAtk})");
            // 어떤 오브젝트의 어떤 컴포넌트인지 명확히 출력
            Debug.Log($"[DEBUG] {gameObject.name}이 {mState.gameObject.name}의 MechaState.FinalAtk({mState.FinalAtk})를 가져옴");
        }
        else if (pState != null)
        {
            ownerAtk = pState.FinalAtk;
            isInitialized = true;
            Debug.Log($"<color=yellow>[Hitbox]</color> {gameObject.name}: 플레이어 스탯 확인 (Atk: {ownerAtk})");
        }
        else
        {
            // 부모 중에 State가 없으면 (폭발 프리팹 등) SetOwnerAtk를 기다림
            isInitialized = false;
            Debug.Log($"<color=white>[Hitbox]</color> {gameObject.name}: 부모 State 없음. 외부 주입 대기 중.");
        }
    }

    // [핵심] 외부(드론, 폭탄 등)에서 공격력을 직접 넣어줄 때 사용
    public void SetOwnerAtk(float atk)
    {
        ownerAtk = atk;
        isInitialized = true; // 이제 공격 가능 상태

        Debug.Log($"<color=lime>[Hitbox]</color> {gameObject.name}: 공격력 주입 완료 (값: {ownerAtk})");

        // 주입된 즉시 판정 시작 (중복 히트 방지를 위해 목록 비우고 실행)
        hitTargets.Clear();
        CheckImmediateOverlap();
    }

    private void OnEnable()
    {
        hitTargets.Clear();

        // 초기화가 완료된 녀석(이미 부모가 있는 근접 무기 등)만 켜지자마자 판정 시작
        // 초기화 안 된 폭발물은 여기서 판정하지 않으므로 '50 대미지' 버그가 예방됨
        if (isInitialized)
        {
            CheckImmediateOverlap();
        }
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
        // 2. 초기화 가드: 설정이 끝나기 전엔 절대 타격하지 않음
        if (!isInitialized || ownerAtk <= 0) return;

        if (((1 << collision.gameObject.layer) & targetLayers) == 0) return;
        if (hitTargets.Contains(collision.gameObject)) return;
        if (!canHitMultiple && hitTargets.Count > 0) return;

        float finalDamage = ownerAtk * damageMultiplier;
        IDamageable target = collision.GetComponent<IDamageable>();

        if (target != null)
        {
            hitTargets.Add(collision.gameObject);
            Vector2 hitDir = (collision.transform.position - transform.position).normalized;
            target.TakeDamage(finalDamage, knockbackPower, hitDir);
            OnHitEffect?.Invoke(collision.transform.position);

            Debug.Log($"<color=white>[Hit]</color> {collision.gameObject.name}에게 {finalDamage} 대미지 전달!");
        }
    }

    public void DestroySelf() => Destroy(gameObject);

    public void ResetHitbox()
    {
        hitTargets.Clear();
        if (isInitialized) CheckImmediateOverlap();
    }
}