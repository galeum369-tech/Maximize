using UnityEngine;

public class DroneState : MonoBehaviour
{
    public PlayerData data; // 인스펙터에서 PlayerData SO 연결

    [Header("드론 실시간 스탯")]
    public float currentAtk;
    public float fireRate;
    public float range;

    [Header("스킬 쿨타임 관리")]
    public float skill1Cooldown = 5f;
    public float skill2Cooldown = 10f;

    [HideInInspector] public float skill1Timer;
    [HideInInspector] public float skill2Timer;

    private void OnEnable()
    {
        // 드론이 활성화될 때(인간 모드로 전환될 때) 스탯 갱신
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (data == null) return;

        // PlayerData에 정의된 드론 전용 스탯 로드
        currentAtk = data.GetDroneAtk();
        fireRate = data.droneFireRate; // PlayerData에 선언된 기본 발사 간격
        range = data.droneRange;       // PlayerData에 선언된 탐색 사거리

        Debug.Log($"<color=lime>[Drone]</color> 스탯 갱신 완료 - Atk: {currentAtk}, FireRate: {fireRate}");
    }

    private void Update()
    {
        // 스킬 쿨타임 계산
        if (skill1Timer > 0) skill1Timer -= Time.deltaTime;
        if (skill2Timer > 0) skill2Timer -= Time.deltaTime;
    }
}