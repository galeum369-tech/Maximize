using UnityEngine;

public class DroneState : MonoBehaviour
{
    public PlayerData data;

    [Header("드론 실시간 스탯")]
    public float currentAtk;
    public float fireRate;
    public float range;

    // [수정] 쿨타임 관련 타이머 변수 및 Update문 모두 제거됨

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (data == null) return;

        currentAtk = data.GetDroneAtk();
        fireRate = data.droneFireRate;
        range = data.droneRange;

        Debug.Log($"<color=lime>[Drone]</color> 스탯 갱신 완료 - Atk: {currentAtk}, FireRate: {fireRate}");
    }
}