using UnityEngine;
using System.Collections.Generic;

// 조합/구매에 필요한 재료 정보
[System.Serializable]
public class CraftIngredient
{
    public ItemData item;
    public int amount;
}

// 상점 진열대 구조
[System.Serializable]
public class ShopEntry
{
    public ItemData resultItem; // 유저가 받게 될 아이템
    public int buyPrice;        // 유저가 내야 할 골드
    public List<CraftIngredient> requiredMaterials = new List<CraftIngredient>();
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("상점 판매 목록 (진열대)")]
    public List<ShopEntry> shopEntries = new List<ShopEntry>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // [구매 로직]
    public void BuyItem(int shopIndex)
    {
        if (shopIndex < 0 || shopIndex >= shopEntries.Count) return;

        ShopEntry entry = shopEntries[shopIndex];
        if (entry == null || entry.resultItem == null) return;

        // 1. 골드 검사
        if (GameManager.Instance.currentMoney < entry.buyPrice)
        {
            Debug.Log("골드가 부족해!");
            return;
        }

        // 2. 재료 검사
        foreach (var ingredient in entry.requiredMaterials)
        {
            if (!InventoryManager.Instance.HasItems(ingredient.item, ingredient.amount))
            {
                Debug.Log($"{ingredient.item.itemName} 재료가 부족해!");
                return;
            }
        }

        // 3. 가방 공간 확인 및 지급
        if (InventoryManager.Instance.AddItem(entry.resultItem, 1))
        {
            GameManager.Instance.UseMoney(entry.buyPrice);

            foreach (var ingredient in entry.requiredMaterials)
            {
                InventoryManager.Instance.ConsumeItems(ingredient.item, ingredient.amount);
            }

            Debug.Log($"{entry.resultItem.itemName} 구매 완료!");
        }
        else
        {
            Debug.Log("가방이 꽉 차서 살 수 없어!");
        }
    }

    // [판매 로직]
    public void SellItem(int inventoryIndex)
    {
        ItemSlot invSlot = InventoryManager.Instance.slots[inventoryIndex];
        if (invSlot.item == null) return;

        int goldToGive = invSlot.item.sellPrice;

        GameManager.Instance.currentMoney += goldToGive;
        Debug.Log($"{invSlot.item.itemName} 판매 완료! (+{goldToGive} G)");

        invSlot.count--;
        if (invSlot.count <= 0) invSlot.Clear();
    }
}