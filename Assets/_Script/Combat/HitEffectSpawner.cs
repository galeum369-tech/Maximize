using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    public GameObject effectPrefab; // 팡! 터지는 이펙트 프리팹
    private UniversalHitbox hitbox;

    private void Awake()
    {
        hitbox = GetComponent<UniversalHitbox>();
    }

    private void OnEnable()
    {
        // 히트박스 이벤트 구독
        hitbox.OnHitEffect += SpawnEffect;
    }

    private void OnDisable()
    {
        // 구독 해제 (메모리 누수 방지)
        hitbox.OnHitEffect -= SpawnEffect;
    }

    private void SpawnEffect(Vector2 position)
    {
        if (effectPrefab != null)
        {
            // 계산된 정확한 타격 지점에 이펙트 생성
            Instantiate(effectPrefab, position, Quaternion.identity);
        }
    }
}