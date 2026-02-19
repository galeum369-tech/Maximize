using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<ItemSlot> slots = new List<ItemSlot>();
    public int maxSlotCount = 40;

    private void Awake()
    {
        Instance = this;
        // 40개의 빈 슬롯 초기화
        for (int i = 0; i < maxSlotCount; i++) slots.Add(new ItemSlot());
    }

    // 아이템 획득 로직
    public bool AddItem(ItemData data, int amount)
    {
        // 1. 기존 슬롯에 합치기 (장비 제외)
        if (data.itemType != ItemType.Equipment)
        {
            foreach (var slot in slots)
            {
                if (slot.item == data && slot.count < data.GetMaxStack())
                {
                    int canAdd = data.GetMaxStack() - slot.count;
                    int toAdd = Mathf.Min(canAdd, amount);
                    slot.count += toAdd;
                    amount -= toAdd;
                    if (amount <= 0) return true;
                }
            }
        }

        // 2. 빈 슬롯에 새로 추가
        while (amount > 0)
        {
            ItemSlot emptySlot = slots.Find(s => s.item == null);
            if (emptySlot != null)
            {
                emptySlot.item = data;
                int toAdd = Mathf.Min(amount, data.GetMaxStack());
                emptySlot.count = toAdd;
                amount -= toAdd;
            }
            else
            {
                Debug.Log("인벤토리 꽉 참!");
                return false;
            }
        }
        return true;
    }

    // 사망 패널티: 장비 제외 재료템 25% 소실 (내림 적용)
    public void HandleDeathPenalty()
    {
        foreach (var slot in slots)
        {
            // 아이템이 있고 장비가 아닐 때만 계산
            if (slot.item != null && slot.item.itemType != ItemType.Equipment)
            {
                // 25% 계산 후 소수점 내림 처리
                // 계산식: $loss = \lfloor slot.count \times 0.25 \rfloor$
                int loss = Mathf.FloorToInt(slot.count * 0.25f);

                if (loss > 0)
                {
                    slot.count -= loss;
                    Debug.Log($"{slot.item.itemName} 아이템을 {loss}개 잃었습니다.");
                }

                // 수량이 0이 되면 슬롯 비우기
                if (slot.count <= 0)
                {
                    slot.Clear();
                }
            }
        }
        Debug.Log("사망 패널티 처리가 완료되었습니다.");
    }
}

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int count;
    public void Clear() { item = null; count = 0; }
}