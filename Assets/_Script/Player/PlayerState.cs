using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [Header("데이터 참조")]
    public PlayerData baseData;

    [Header("장착 슬롯")]
    public EquipmentData coreSlot;
    public EquipmentData frameSlot;
    public EquipmentData gearSlot;
    public EquipmentData chipSlot;

    // 최종 스탯
    public float FinalMaxHP { get; private set; }
    public float FinalAtk { get; private set; }
    public float FinalCrit { get; private set; }
    public float FinalDef { get; private set; }
    public float FinalSpd { get; private set; }

    [Header("현재 상태")]
    public float currentHp;

    private void Awake()
    {
        RecalculateFinalStats();
        currentHp = FinalMaxHP;
    }

    public void Equip(EquipmentData newItem)
    {
        if (newItem == null) return;
        switch (newItem.equipType)
        {
            case EquipType.Core: coreSlot = newItem; break;
            case EquipType.Frame: frameSlot = newItem; break;
            case EquipType.Gear: gearSlot = newItem; break;
            case EquipType.Chip: chipSlot = newItem; break;
        }
        RecalculateFinalStats();
    }

    public void RecalculateFinalStats()
    {
        FinalMaxHP = baseData.GetHp() + GetTotalBonus("hp");
        FinalAtk = baseData.GetAtk() + GetTotalBonus("atk");
        FinalCrit = baseData.GetCrit() + GetTotalBonus("crit");
        FinalDef = baseData.GetDef() + GetTotalBonus("def");
        FinalSpd = baseData.GetSpd() + GetTotalBonus("spd");

        currentHp = Mathf.Min(currentHp, FinalMaxHP);
    }

    private float GetTotalBonus(string statType)
    {
        return GetBonus(coreSlot, statType) + GetBonus(frameSlot, statType) +
               GetBonus(gearSlot, statType) + GetBonus(chipSlot, statType);
    }

    private float GetBonus(EquipmentData item, string statType)
    {
        if (item == null) return 0;
        switch (statType)
        {
            case "hp": return item.hpBonus;
            case "atk": return item.atkBonus;
            case "crit": return item.critBonus;
            case "def": return item.defBonus;
            case "spd": return item.spdBonus;
            default: return 0;
        }
    }
}