using UnityEngine;

public class DroneState : MonoBehaviour
{
    public PlayerData data;

    [Header("드론 실시간 스탯")]
    public float currentAtk;
    public float fireRate;
    public float range;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += RefreshStats;

        RefreshStats();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= RefreshStats;
    }

    public void RefreshStats()
    {
        if (data == null) return;

        // 드론 기본 공격력 + 장비에서 오는 드론 보너스
        currentAtk = data.GetDroneAtk() + InventoryManager.Instance.GetTotalBonus("Drone", "atk");
        fireRate = data.droneFireRate;
        range = data.droneRange;

        Debug.Log($"<color=lime>[Drone]</color> 스탯 갱신 완료 - Atk: {currentAtk}");
    }
}