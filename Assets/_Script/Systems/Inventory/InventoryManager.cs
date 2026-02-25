using UnityEngine;
using System.Collections.Generic;
using System;

// 인벤토리 한 칸의 데이터를 담당하는 클래스
[System.Serializable]
public class ItemSlot
{
    public ItemData item; // 아이템 원본 데이터 (SO)
    public int count;     // 현재 겹쳐진 개수

    // 슬롯 비우기
    public void Clear()
    {
        item = null;
        count = 0;
    }
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("가방 데이터")]
    public List<ItemSlot> slots = new List<ItemSlot>();
    public int maxSlotCount = 40;

    [Header("장착된 장비 (0:Core, 1:Frame, 2:Gear, 3:Chip)")]
    public EquipmentData[] humanEquips = new EquipmentData[4];
    public EquipmentData[] mechaEquips = new EquipmentData[4];

    // 장비가 바뀌었을 때 각 State들에게 스탯 재계산하라고 알리는 이벤트
    public event Action OnEquipmentChanged;

    private void Awake()
    {
        Instance = this;
        // 시작할 때 빈 슬롯 40개 생성
        for (int i = 0; i < maxSlotCount; i++) slots.Add(new ItemSlot());
    }

    // 아이템 획득
    public bool AddItem(ItemData data, int amount)
    {
        // 1. 장비가 아니라면 기존 슬롯에 합치기 시도
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
                    if (amount <= 0) return true; // 다 넣었으면 성공
                }
            }
        }

        // 2. 남은 수량은 빈 슬롯을 찾아서 새로 추가
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
                Debug.Log("인벤토리가 꽉 찼습니다!");
                return false;
            }
        }
        return true;
    }

    // 사망 패널티 (재료템 25% 소실)
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

    // 인벤토리 내 아이템 이동, 교체, 병합
    public void MoveOrSwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;

        ItemSlot fromSlot = slots[fromIndex];
        ItemSlot toSlot = slots[toIndex];

        // 1. 병합 시도 (같은 아이템이고 스택 가능한 경우)
        if (fromSlot.item != null && toSlot.item != null &&
            fromSlot.item == toSlot.item &&
            fromSlot.item.GetMaxStack() > 1)
        {
            int availableSpace = toSlot.item.GetMaxStack() - toSlot.count;

            if (availableSpace > 0)
            {
                int moveAmount = Mathf.Min(availableSpace, fromSlot.count);
                toSlot.count += moveAmount;
                fromSlot.count -= moveAmount;

                if (fromSlot.count <= 0) fromSlot.Clear();
                return;
            }
        }

        // 2. 단순 자리 교체 (Swap)
        ItemSlot temp = new ItemSlot();
        temp.item = fromSlot.item;
        temp.count = fromSlot.count;

        fromSlot.item = toSlot.item;
        fromSlot.count = toSlot.count;

        toSlot.item = temp.item;
        toSlot.count = temp.count;
    }

    // 장비 장착 처리
    public void EquipItem(EquipmentData equip, bool isMechaMode)
    {
        EquipmentData[] targetEquips = isMechaMode ? mechaEquips : humanEquips;
        int slotIndex = (int)equip.equipType;

        // 1. 이미 그 부위에 장비가 있다면 가방으로 다시 넣기
        if (targetEquips[slotIndex] != null)
        {
            AddItem(targetEquips[slotIndex], 1);
        }

        // 2. 새 장비 장착
        targetEquips[slotIndex] = equip;

        // 3. 스탯 재계산 이벤트 발생 (구독 중인 PlayerState 등이 알아서 갱신됨)
        OnEquipmentChanged?.Invoke();
    }

    // 통합 스탯 보너스 계산기 (각 State들이 호출해서 씀)
    public float GetTotalBonus(string targetType, string statType)
    {
        float total = 0f;

        // 인간 장비 검사
        foreach (var equip in humanEquips)
        {
            if (equip != null) total += ExtractStat(equip, targetType, statType);
        }

        // 메카 장비 검사
        foreach (var equip in mechaEquips)
        {
            if (equip != null) total += ExtractStat(equip, targetType, statType);
        }

        return total;
    }

    public void UnequipItem(int equipSlotIndex, bool isMechaMode)
    {
        EquipmentData[] targetEquips = isMechaMode ? mechaEquips : humanEquips;
        EquipmentData itemToUnequip = targetEquips[equipSlotIndex];

        if (itemToUnequip != null)
        {
            // 1. 가방에 넣기 시도
            bool added = AddItem(itemToUnequip, 1);

            if (added)
            {
                // 2. 가방에 성공적으로 들어갔으면 장착 칸 비우기
                targetEquips[equipSlotIndex] = null;

                // 3. 스탯 재계산
                OnEquipmentChanged?.Invoke();
                Debug.Log($"{itemToUnequip.itemName} 장착 해제 완료!");
            }
            else
            {
                Debug.Log("가방이 꽉 차서 장비를 해제할 수 없습니다!");
            }
        }
    }

    // 개별 장비에서 스탯 뽑아오기
    private float ExtractStat(EquipmentData equip, string targetType, string statType)
    {
        // 타겟이 Player이거나 Mecha일 때 기본 보너스 부여
        // (추후 드론 전용 옵션이 생기면 targetType == "Drone" 조건 추가 가능)
        if (targetType == "Player" || targetType == "Mecha")
        {
            switch (statType)
            {
                case "hp": return equip.hpBonus;
                case "atk": return equip.atkBonus;
                case "def": return equip.defBonus;
                case "spd": return equip.spdBonus;
            }
        }
        return 0f;
    }
}