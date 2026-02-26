using UnityEngine;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descriptionText;
    public GameObject tooltipRoot;

    public void Show(ItemData item, ShopEntry shopEntry = null)
    {
        if (item == null)
        {
            Hide();
            return;
        }

        tooltipRoot.SetActive(true);

        itemNameText.text = item.itemName;
        rarityText.text = UIUtils.GetRarityName(item.rarity);

        Color rarityColor = UIUtils.GetRarityColor(item.rarity);
        itemNameText.color = rarityColor;
        rarityText.color = rarityColor;

        string desc = item.description;

        if (item.itemType == ItemType.Equipment)
        {
            if (!string.IsNullOrEmpty(desc)) desc += "\n\n";

            // --- 인간 스탯 ---
            if (item.humanHpBonus != 0 || item.humanAtkBonus != 0 || item.humanDefBonus != 0 || item.humanSpdBonus != 0)
            {
                desc += "<color=#ADD8E6>[인간]</color>\n";
                if (item.humanHpBonus != 0) desc += $"체력 +{item.humanHpBonus}\n";
                if (item.humanAtkBonus != 0) desc += $"공격력 +{item.humanAtkBonus}\n";
                if (item.humanDefBonus != 0) desc += $"방어력 +{item.humanDefBonus}\n";
                if (item.humanSpdBonus != 0) desc += $"이동속도 +{item.humanSpdBonus}\n";
            }

            // --- 메카 스탯 ---
            if (item.mechaHpBonus != 0 || item.mechaAtkBonus != 0 || item.mechaDefBonus != 0 || item.mechaSpdBonus != 0)
            {
                desc += "<color=#FFA07A>[메카]</color>\n";
                if (item.mechaHpBonus != 0) desc += $"체력 +{item.mechaHpBonus}\n";
                if (item.mechaAtkBonus != 0) desc += $"공격력 +{item.mechaAtkBonus}\n";
                if (item.mechaDefBonus != 0) desc += $"방어력 +{item.mechaDefBonus}\n";
                if (item.mechaSpdBonus != 0) desc += $"이동속도 +{item.mechaSpdBonus}\n";
            }

            // --- 드론 스탯 ---
            if (item.droneAtkBonus != 0 || item.droneFireRateBonus != 0 || item.droneRangeBonus != 0)
            {
                desc += "<color=#98FB98>[드론]</color>\n";
                if (item.droneAtkBonus != 0) desc += $"공격력 +{item.droneAtkBonus}\n";
                if (item.droneFireRateBonus != 0) desc += $"발사속도 +{item.droneFireRateBonus}\n";
                if (item.droneRangeBonus != 0) desc += $"사거리 +{item.droneRangeBonus}\n";
            }
        }

        // ==========================================
        // 상점/가방에 따른 가격 및 재료 표시
        // ==========================================
        if (InventoryUI.Instance.isShopMode && shopEntry != null)
        {
            desc += "\n\n<color=#FFD700>[구매 필요 조건]</color>\n";

            if (shopEntry.buyPrice > 0)
                desc += $"- 골드: {shopEntry.buyPrice} G\n";

            foreach (var mat in shopEntry.requiredMaterials)
            {
                desc += $"- {mat.item.itemName} x{mat.amount}\n";
            }
        }
        else if (!InventoryUI.Instance.isShopMode)
        {
            if (item.sellPrice > 0)
                desc += $"\n\n<color=#FFD700>판매가: {item.sellPrice} G</color>";
            else
                desc += "\n\n<color=#808080>판매 불가</color>";
        }

        descriptionText.text = desc;
    }

    public void Hide()
    {
        tooltipRoot.SetActive(false);
    }
}