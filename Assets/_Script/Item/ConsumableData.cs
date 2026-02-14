using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Item/Consumable")]
public class ConsumableData : ItemData // 이것도 ItemData를 상속
{
    public float healAmount; // 회복량
    public float effectDuration; // 버프 지속 시간
    // 여기에 사용 시 발생하는 특수 로직 연결
}