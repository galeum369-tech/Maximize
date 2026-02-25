using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData baseData;

    // 최종 스탯
    public float FinalMaxHP { get; private set; }
    public float FinalAtk { get; private set; }
    public float FinalDef { get; private set; }
    public float FinalSpd { get; private set; }

    [Header("현재 상태")]
    public float currentHp;

    private void Start()
    {
        RecalculateFinalStats();
        currentHp = FinalMaxHP;
    }

    private void OnEnable()
    {
        // 인벤토리에서 장비가 변경될 때마다 자동 재계산 구독
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged += RecalculateFinalStats;
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnEquipmentChanged -= RecalculateFinalStats;
    }

    public void RecalculateFinalStats()
    {
        // 인벤토리 매니저에서 "Player" 대상의 보너스 수치를 싹 긁어옴
        FinalMaxHP = baseData.GetHp() + InventoryManager.Instance.GetTotalBonus("Player", "hp");
        FinalAtk = baseData.GetAtk() + InventoryManager.Instance.GetTotalBonus("Player", "atk");
        FinalDef = baseData.GetDef() + InventoryManager.Instance.GetTotalBonus("Player", "def");
        FinalSpd = baseData.GetSpd() + InventoryManager.Instance.GetTotalBonus("Player", "spd");

        currentHp = Mathf.Min(currentHp, FinalMaxHP);
        Debug.Log($"[Player] 스탯 갱신 완료! ATK: {FinalAtk}");
    }
}