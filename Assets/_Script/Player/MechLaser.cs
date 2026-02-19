using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class MechaLaser : MonoBehaviour
{
    [Header("컴포넌트 연결")]
    private LineRenderer lineRenderer;
    public GameObject hitEffect; // 레이저 끝점에 생길 이펙트

    [Header("사거리 및 레이어")]
    public float maxDistance = 20f;
    public LayerMask obstacleLayer; // 벽, 바닥 (메카 본체 레이어는 제외할 것)
    public LayerMask enemyLayer;    // 적 레이어

    [Header("데미지 설정")]
    public float damageMultiplier = 0.5f;
    public float damageTick = 0.1f;
    public float knockbackPower = 1f;

    [Header("물리 및 스케일 보정")]
    [Tooltip("레이저 시작 지점을 총구보다 약간 앞으로 밀어 자기 자신 충돌을 방지합니다.")]
    public float startOffset = 0.2f;

    private float ownerAtk;
    private float tickTimer;
    private bool isActive;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        // 시작할 때는 완전히 비활성화
        Deactivate();
    }

    public void Activate(float atk)
    {
        ownerAtk = atk;
        isActive = true;
        if (lineRenderer) lineRenderer.enabled = true;
        tickTimer = 0;
        gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        isActive = false;
        if (lineRenderer) lineRenderer.enabled = false;
        if (hitEffect) hitEffect.SetActive(false);
        gameObject.SetActive(false); // 오브젝트 자체를 꺼서 Update 중단
    }

    private void Update()
    {
        if (!isActive) return;

        // 1. 레이캐스트 로직 (월드 좌표계에서 거리 계산)
        Vector2 origin = (Vector2)transform.position + ((Vector2)transform.right * startOffset);
        RaycastHit2D hitObstacle = Physics2D.Raycast(origin, transform.right, maxDistance, obstacleLayer);

        // [디버그] 씬 뷰에서 레이저 판정 선을 그립니다
        Debug.DrawRay(origin, transform.right * (hitObstacle.collider ? hitObstacle.distance : maxDistance),
                      hitObstacle.collider ? Color.green : Color.red);

        float currentDist = maxDistance;
        if (hitObstacle.collider != null)
        {
            currentDist = startOffset + hitObstacle.distance;

            if (hitEffect)
            {
                hitEffect.SetActive(true);
                hitEffect.transform.position = hitObstacle.point;
                hitEffect.transform.rotation = Quaternion.FromToRotation(Vector2.up, hitObstacle.normal);
            }
        }
        else
        {
            if (hitEffect) hitEffect.SetActive(false);
        }

        // --- [핵심 수정] 2. 라인 렌더러 그리기 (스케일 보정 적용) ---
        // 부모의 스케일(5배 등)이 자식의 로컬 좌표에 영향을 주므로, 월드 거리를 스케일로 나눕니다.
        float globalScaleX = transform.lossyScale.x;
        if (Mathf.Abs(globalScaleX) < 0.01f) globalScaleX = 1f; // 0으로 나누기 방지

        float localDist = currentDist / globalScaleX; // 로컬 좌표상에서의 실제 거리 계산

        lineRenderer.SetPosition(0, Vector3.zero);
        lineRenderer.SetPosition(1, Vector3.right * localDist);

        // 3. 다단 히트 데미지 처리
        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTick)
        {
            tickTimer = 0f;
            ApplyLaserDamage(currentDist);
        }

        // 4. 비주얼 효과
        if (lineRenderer.material.HasProperty("_MainTex"))
        {
            float offset = Time.time * -5f;
            lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);
        }
    }

    private void ApplyLaserDamage(float dist)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, transform.right, dist, enemyLayer);

        foreach (var h in hits)
        {
            IDamageable target = h.collider.GetComponent<IDamageable>();
            if (target != null)
            {
                float finalDamage = ownerAtk * damageMultiplier;
                target.TakeDamage(finalDamage, knockbackPower, transform.right);
            }
        }
    }
}