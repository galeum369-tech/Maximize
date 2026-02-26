using UnityEngine;

// --- 모든 아이템 관련 Enum을 여기에 모아둠 ---
public enum ItemRarity { Common, Uncommon, Rare, Epic }
public enum ItemType { Equipment, Material, Consumable }
public enum EquipType { Core, Frame, Gear, Chip }
public enum ConsumableType { HealHP, HealEnergy, Buff }
public enum BuffStatType { None, Atk, Def, Spd }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;
    public int id;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;

    // 상점 분리용 (이제 아이템은 '팔 때 얼마인가'만 알면 됨!)
    public int sellPrice;

    [TextArea(3, 5)]
    public string description;

    // =====================================
    // 장비(Equipment) 전용 데이터
    // =====================================
    [Header("장비 설정 (ItemType이 Equipment일 때만 적용)")]
    public EquipType equipType;

    public float humanAtkBonus;
    public float humanHpBonus;
    public float humanDefBonus;
    public float humanSpdBonus;

    public float mechaAtkBonus;
    public float mechaHpBonus;
    public float mechaDefBonus;
    public float mechaSpdBonus;

    public float droneAtkBonus;
    public float droneFireRateBonus;
    public float droneRangeBonus;

    // =====================================
    // 소비(Consumable) 전용 데이터
    // =====================================
    [Header("소모품 설정 (ItemType이 Consumable일 때만 적용)")]
    public ConsumableType consumableType;
    public BuffStatType buffStatType;

    [Tooltip("회복량 또는 버프 증가 수치")]
    public float effectValue;

    [Tooltip("버프 아이템일 경우 지속 시간 (초)")]
    public float effectDuration;

    // =====================================

    public int GetMaxStack()
    {
        if (itemType == ItemType.Equipment) return 1;

        return rarity switch
        {
            ItemRarity.Common => 20,
            ItemRarity.Uncommon => 10,
            ItemRarity.Rare => 5,
            ItemRarity.Epic => 1,
            _ => 1
        };
    }
}