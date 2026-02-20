using UnityEngine;

public class MechaLaser : MonoBehaviour
{
    [Header("컴포넌트 및 비주얼")]
    public Transform laserVisual;      // [자식] Sprite + Collider + UniversalHitbox가 있는 오브젝트
    public GameObject hitEffect;       // [자식] 충격 지점 이펙트

    [Header("설정")]
    public float maxDistance = 20f;
    public LayerMask obstacleLayer;    // 지형 레이어
    public float damageTick = 0.1f;    // 다단 히트 주기

    private UniversalHitbox hitbox;
    private float tickTimer;
    private bool isActive;

    private void Awake()
    {
        if (laserVisual != null)
        {
            hitbox = laserVisual.GetComponent<UniversalHitbox>();
        }
    }

    public void Activate(float atk)
    {
        isActive = true;
        gameObject.SetActive(true);

        // 유니버설 히트박스에 공격력 주입
        if (hitbox != null) hitbox.SetOwnerAtk(atk);

        tickTimer = 0;
    }

    public void Deactivate()
    {
        isActive = false;
        if (hitEffect) hitEffect.SetActive(false);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isActive) return;

        // 1. 레이캐스트로 실제 거리 측정 (지형 충돌용)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.right, maxDistance, obstacleLayer);
        float currentDist = hit.collider != null ? hit.distance : maxDistance;

        // 2. [핵심] 자식(Visual) 스케일 조절 -> 콜라이더도 자동으로 같이 늘어남
        if (laserVisual != null)
        {
            // 부모 스케일 영향을 받지 않도록 Abs로 보정
            float globalScaleX = Mathf.Abs(transform.lossyScale.x);
            if (globalScaleX < 0.01f) globalScaleX = 1f;

            laserVisual.localScale = new Vector3(currentDist / globalScaleX, 1, 1);
        }

        // 3. 충격 지점 이펙트 위치 업데이트
        if (hitEffect != null)
        {
            if (hit.collider != null)
            {
                hitEffect.SetActive(true);
                hitEffect.transform.position = hit.point;
                hitEffect.transform.up = hit.normal;
            }
            else hitEffect.SetActive(false);
        }

        // 4. [중요] 다단 히트 처리
        // 유니버설 히트박스는 기본적으로 한 번만 때리므로, 
        // 틱마다 hitTargets 리스트를 비워줘서 다시 때릴 수 있게 함
        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTick)
        {
            tickTimer = 0f;
            if (hitbox != null) hitbox.ResetHitbox();
        }
    }
}