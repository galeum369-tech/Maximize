using UnityEngine;

public enum EquipType
{
    Core,
    Frame,
    Gear,
    Chip
}

[CreateAssetMenu(fileName = "new EquipmentData", menuName = "Item/EquipmentData")]
public class EquipmentData : ItemData
{
    public EquipType equipType;

    [Header("스텟 보너스")]
    public float atkBonus;
    public float critBonus;
    public float hpBonus;
    public float defBonus;
    public float spdBonus;
    public float staminaBonus;
    
}
