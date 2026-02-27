using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class CraftIngredient
{
    public ItemData item;
    public int amount;
}

[System.Serializable]
public class ShopEntry
{
    public ItemData resultItem;
    public int buyPrice;
    public List<CraftIngredient> requiredMaterials = new List<CraftIngredient>();
}

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("상점 판매 목록 (진열대)")]
    public List<ShopEntry> shopEntries = new List<ShopEntry>();

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void BuyItem(int shopIndex)
    {
        if (shopIndex < 0 || shopIndex >= shopEntries.Count) return;

        ShopEntry entry = shopEntries[shopIndex];
        if (entry == null || entry.resultItem == null) return;

        if (GameManager.Instance.currentMoney < entry.buyPrice)
        {
            Debug.Log("골드가 부족해!");
            return;
        }

        foreach (var ingredient in entry.requiredMaterials)
        {
            if (!InventoryManager.Instance.HasItems(ingredient.item, ingredient.amount))
            {
                Debug.Log($"{ingredient.item.itemName} 재료가 부족해!");
                return;
            }
        }

        if (InventoryManager.Instance.AddItem(entry.resultItem, 1))
        {
            GameManager.Instance.UseMoney(entry.buyPrice);

            foreach (var ingredient in entry.requiredMaterials)
            {
                InventoryManager.Instance.ConsumeItems(ingredient.item, ingredient.amount);
            }

            Debug.Log($"{entry.resultItem.itemName} 구매 완료!");
        }
        else Debug.Log("가방이 꽉 차서 살 수 없어!");
    }

    public void SellItem(int inventoryIndex)
    {
        ItemSlot invSlot = InventoryManager.Instance.slots[inventoryIndex];
        if (invSlot.item == null) return;

        int goldToGive = invSlot.item.sellPrice;

        // [핵심 수정] 변수에 직접 더하지 않고 AddMoney 이벤트를 호출! (UI 즉시 갱신됨)
        GameManager.Instance.AddMoney(goldToGive);
        Debug.Log($"{invSlot.item.itemName} 판매 완료! (+{goldToGive} G)");

        invSlot.count--;
        if (invSlot.count <= 0) invSlot.Clear();
    }
}