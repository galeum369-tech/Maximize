using UnityEngine;

public enum ItemRarity { Common, Uncommon, Rare, Epic }
public enum ItemType { Equipment, Material, Consumable }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int id;
    public Sprite icon;
    public ItemType itemType;
    public ItemRarity rarity;
    public int price;

    // 등급별 스택 제한 (커먼 20, 언커먼 10, 레어 5, 에픽 1)
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