using UnityEngine;
using System.Collections.Generic;

public class StorageManager : MonoBehaviour
{
    public static StorageManager Instance { get; private set; }

    [Header("무한 창고 데이터")]
    // 칸 제한 없이 계속 Add() 할 수 있는 리스트
    public List<ItemSlot> storageSlots = new List<ItemSlot>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // 가방 -> 창고로 넣기
    public void DepositItem(int inventoryIndex)
    {
        ItemSlot invSlot = InventoryManager.Instance.slots[inventoryIndex];
        if (invSlot.item == null) return;

        // 1. 장비가 아니라면 창고에 있는 같은 아이템에 먼저 겹치기 시도
        if (invSlot.item.itemType != ItemType.Equipment)
        {
            foreach (var sSlot in storageSlots)
            {
                if (sSlot.item == invSlot.item && sSlot.count < invSlot.item.GetMaxStack())
                {
                    int canAdd = invSlot.item.GetMaxStack() - sSlot.count;
                    int toAdd = Mathf.Min(canAdd, invSlot.count);
                    sSlot.count += toAdd;
                    invSlot.count -= toAdd;

                    // 다 넣었으면 가방 칸 비우고 종료
                    if (invSlot.count <= 0)
                    {
                        invSlot.Clear();
                        return;
                    }
                }
            }
        }

        // 2. 겹치고도 남았거나, 장비템이거나, 창고에 없던 새 아이템이라면 창고 끝에 새 칸을 무한히 추가!
        if (invSlot.count > 0)
        {
            ItemSlot newSlot = new ItemSlot();
            newSlot.item = invSlot.item;
            newSlot.count = invSlot.count;
            storageSlots.Add(newSlot); // 리스트 길이 +1

            invSlot.Clear(); // 가방 칸 비우기
        }
    }

    // 창고 -> 가방으로 빼기
    public void WithdrawItem(int storageIndex)
    {
        if (storageIndex < 0 || storageIndex >= storageSlots.Count) return;

        ItemSlot sSlot = storageSlots[storageIndex];
        if (sSlot.item == null) return;

        // 가방(InventoryManager)의 AddItem 로직을 그대로 사용해서 쑤셔넣음
        bool success = InventoryManager.Instance.AddItem(sSlot.item, sSlot.count);

        if (success)
        {
            // 가방에 성공적으로 들어갔으면 창고 리스트에서 해당 칸을 아예 날려버림 (빈 칸 안 남기기)
            storageSlots.RemoveAt(storageIndex);
            Debug.Log("창고에서 아이템 꺼내기 성공!");
        }
        else
        {
            Debug.Log("가방이 꽉 차서 창고에서 꺼낼 수 없어!");
        }
    }
}