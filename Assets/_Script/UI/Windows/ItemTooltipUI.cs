using UnityEngine;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descriptionText;
    public GameObject tooltipRoot;

    public void Show(ItemData item)
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

        string desc = "";
        if (item is EquipmentData equip)
        {
            if (equip.atkBonus != 0) desc += $"공격력 +{equip.atkBonus}\n";
            if (equip.hpBonus != 0) desc += $"체력 +{equip.hpBonus}\n";
            if (equip.defBonus != 0) desc += $"방어력 +{equip.defBonus}%\n";
            if (equip.spdBonus != 0) desc += $"이동속도 +{equip.spdBonus}\n";
        }

        descriptionText.text = desc;
    }

    public void Hide()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }
}