using UnityEngine;
using System.Collections.Generic;
using System;

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int count;
    public void Clear() { item = null; count = 0; }
}

[DefaultExecutionOrder(-100)]
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("가방 데이터")]
    public List<ItemSlot> slots = new List<ItemSlot>();
    public int maxSlotCount = 40;

    [Header("장착된 장비 (0:Core, 1:Frame, 2:Gear, 3:Chip)")]
    public ItemData[] equippedItems = new ItemData[4];

    public event Action OnEquipmentChanged;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < maxSlotCount; i++) slots.Add(new ItemSlot());
    }

    public bool AddItem(ItemData itemToAdd, int amount)
    {
        if (itemToAdd.itemType != ItemType.Equipment)
        {
            foreach (var slot in slots)
            {
                if (slot.item == itemToAdd && slot.count < itemToAdd.GetMaxStack())
                {
                    int canAdd = itemToAdd.GetMaxStack() - slot.count;
                    int toAdd = Mathf.Min(canAdd, amount);
                    slot.count += toAdd;
                    amount -= toAdd;
                    if (amount <= 0) return true;
                }
            }
        }
        foreach (var slot in slots)
        {
            if (slot.item == null)
            {
                slot.item = itemToAdd;
                int toAdd = Mathf.Min(amount, itemToAdd.GetMaxStack());
                slot.count = toAdd;
                amount -= toAdd;
                if (amount <= 0) return true;
            }
        }
        return false;
    }

    public void EquipItem(ItemData equipItem)
    {
        if (equipItem.itemType != ItemType.Equipment) return;

        int slotIndex = (int)equipItem.equipType;

        if (equippedItems[slotIndex] != null)
        {
            AddItem(equippedItems[slotIndex], 1);
        }

        equippedItems[slotIndex] = equipItem;
        OnEquipmentChanged?.Invoke();
    }

    public void UnequipItem(int slotIndex)
    {
        if (equippedItems[slotIndex] != null)
        {
            if (AddItem(equippedItems[slotIndex], 1))
            {
                equippedItems[slotIndex] = null;
                OnEquipmentChanged?.Invoke();
            }
            else Debug.Log("가방 꽉 차서 못 뺌");
        }
    }

    public void MoveOrSwapSlot(int fromIndex, int toIndex)
    {
        var fromSlot = slots[fromIndex];
        var toSlot = slots[toIndex];

        if (toSlot.item != null && toSlot.item == fromSlot.item && toSlot.item.itemType != ItemType.Equipment)
        {
            int canAdd = toSlot.item.GetMaxStack() - toSlot.count;
            int toAdd = Mathf.Min(canAdd, fromSlot.count);
            toSlot.count += toAdd;
            fromSlot.count -= toAdd;
            if (fromSlot.count <= 0) fromSlot.Clear();
        }
        else
        {
            ItemData tempItem = toSlot.item;
            int tempCount = toSlot.count;
            toSlot.item = fromSlot.item;
            toSlot.count = fromSlot.count;
            fromSlot.item = tempItem;
            fromSlot.count = tempCount;
        }
    }

    public float GetTotalBonus(string targetType, string statType)
    {
        float total = 0f;
        foreach (var equip in equippedItems)
        {
            if (equip != null) total += ExtractStat(equip, targetType, statType);
        }
        return total;
    }

    private float ExtractStat(ItemData equip, string targetType, string statType)
    {
        if (targetType == "Player" || targetType == "Human")
        {
            switch (statType)
            {
                case "hp": return equip.humanHpBonus;
                case "atk": return equip.humanAtkBonus;
                case "def": return equip.humanDefBonus;
                case "spd": return equip.humanSpdBonus;
            }
        }
        else if (targetType == "Mecha")
        {
            switch (statType)
            {
                case "hp": return equip.mechaHpBonus;
                case "atk": return equip.mechaAtkBonus;
                case "def": return equip.mechaDefBonus;
                case "spd": return equip.mechaSpdBonus;
            }
        }
        else if (targetType == "Drone")
        {
            switch (statType)
            {
                case "atk": return equip.droneAtkBonus;
                case "fireRate": return equip.droneFireRateBonus;
                case "range": return equip.droneRangeBonus;
            }
        }
        return 0f;
    }

    public void ForceStatUpdate()
    {
        OnEquipmentChanged?.Invoke();
    }

    public bool HasItems(ItemData data, int amount)
    {
        int currentCount = 0;
        foreach (var slot in slots)
        {
            if (slot.item == data) currentCount += slot.count;
        }
        return currentCount >= amount;
    }

    public void HandleDeathPenalty()
    {
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item.itemType != ItemType.Equipment)
            {
                int loss = Mathf.FloorToInt(slot.count * 0.25f);
                if (loss > 0)
                {
                    slot.count -= loss;
                    Debug.Log($"{slot.item.itemName} 아이템을 {loss}개 잃었습니다.");
                }
                if (slot.count <= 0) slot.Clear();
            }
        }
    }

    public void ConsumeItems(ItemData data, int amount)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].item == data)
            {
                if (slots[i].count >= amount)
                {
                    slots[i].count -= amount;
                    if (slots[i].count <= 0) slots[i].Clear();
                    return;
                }
                else
                {
                    amount -= slots[i].count;
                    slots[i].Clear();
                }
            }
        }
    }
}