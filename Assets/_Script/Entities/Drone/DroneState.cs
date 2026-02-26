using UnityEngine;

public class DroneState : MonoBehaviour
{
    public PlayerData data;

    [Header("드론 실시간 스탯")]
    public float currentAtk;
    public float fireRate;
    public float range;

    private void Start()
    {
        // 다른 매니저들의 Awake가 모두 끝난 안전한 타이밍에 최초 스탯 갱신
        RefreshStats();
    }

    private void OnEnable()
    {
        // 인벤토리가 이미 켜져있을 때만 이벤트 구독 (도중에 껐다 켜질 때를 대비)
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += RefreshStats;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= RefreshStats;
    }

    public void RefreshStats()
    {
        // [핵심] 데이터가 없거나 인벤토리 매니저가 아직 안 생겼으면 에러 안 띄우고 무시함
        if (data == null || InventoryManager.Instance == null) return;

        // 드론 기본 공격력 + 장비에서 오는 드론 보너스
        currentAtk = data.GetDroneAtk() + InventoryManager.Instance.GetTotalBonus("Drone", "atk");
        fireRate = data.droneFireRate;
        range = data.droneRange;

        Debug.Log($"<color=lime>[Drone]</color> 스탯 갱신 완료 - Atk: {currentAtk}");
    }
}