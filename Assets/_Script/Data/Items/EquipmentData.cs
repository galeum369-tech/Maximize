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

    [Header("인간(Human) 스탯 보너스")]
    public float humanAtkBonus;
    public float humanHpBonus;
    public float humanDefBonus;
    public float humanSpdBonus;

    [Header("메카(Mecha) 스탯 보너스")]
    public float mechaAtkBonus;
    public float mechaHpBonus;
    public float mechaDefBonus;
    public float mechaSpdBonus;

    [Header("드론(Drone) 스탯 보너스")]
    public float droneAtkBonus;
    public float droneFireRateBonus; // 예: 발사 속도 보너스
    public float droneRangeBonus;    // 예: 인식 사거리 보너스
}