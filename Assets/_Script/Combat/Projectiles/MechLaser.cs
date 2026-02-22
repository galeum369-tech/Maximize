using UnityEngine;

public class MechaLaser : MonoBehaviour
{
    [Header("컴포넌트 및 비주얼")]
    public Transform laserVisual;      // [자식] Sprite + Collider + UniversalHitbox
    public GameObject hitEffect;       // [자식] 레이저 끝 지점 이펙트

    [Header("설정")]
    public float maxDistance = 20f;    // 고정 사거리
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
        Deactivate();
    }

    public void Activate(float atk)
    {
        isActive = true;
        gameObject.SetActive(true);

        // 공격력 주입 및 히트박스 초기화
        if (hitbox != null)
        {
            hitbox.SetOwnerAtk(atk);
            hitbox.ResetHitbox();
        }

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

        // --- [핵심 수정: 레이캐스트 삭제] ---
        // 지형 충돌 여부 상관없이 항상 maxDistance를 사용함
        float currentDist = maxDistance;

        // 1. 자식(Visual) 스케일 조절 
        // 자식 오브젝트(Sprite+Collider)가 부모 스케일에 상관없이 월드 사거리 20을 유지하도록 계산
        if (laserVisual != null)
        {
            float globalScaleX = Mathf.Abs(transform.lossyScale.x);
            if (globalScaleX < 0.01f) globalScaleX = 1f;

            // 로컬 스케일 = 목표 월드 거리 / 부모의 전역 스케일
            laserVisual.localScale = new Vector3(currentDist / globalScaleX, 1, 1);
        }

        // 2. 이펙트 처리 (필요 없다면 이 부분은 꺼두어도 됨)
        // 레이저의 끝(20m 지점)에 항상 이펙트를 둠
        if (hitEffect != null)
        {
            hitEffect.SetActive(true);
            hitEffect.transform.localPosition = new Vector3(currentDist, 0, 0);
        }

        // 3. 다단 히트 주기적 리셋
        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTick)
        {
            tickTimer = 0f;
            if (hitbox != null) hitbox.ResetHitbox();
        }
    }
}