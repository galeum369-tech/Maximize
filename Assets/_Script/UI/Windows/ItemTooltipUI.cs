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

        // 1. 베이스가 되는 기본 설명 텍스트 (소비/재료템의 경우 이것만 출력됨)
        string desc = item.description;

        // 2. 장비템일 경우 폼별 추가 스탯을 설명 밑에 덧붙여줌
        if (item is EquipmentData equip)
        {
            // 기본 설명이 적혀있다면 장비 스탯과 간격을 띄움
            if (!string.IsNullOrEmpty(desc)) desc += "\n\n";

            // --- 인간 스탯 ---
            if (equip.humanHpBonus != 0 || equip.humanAtkBonus != 0 || equip.humanDefBonus != 0 || equip.humanSpdBonus != 0)
            {
                desc += "<color=#ADD8E6>[인간]</color>\n";
                if (equip.humanHpBonus != 0) desc += $"체력 +{equip.humanHpBonus}\n";
                if (equip.humanAtkBonus != 0) desc += $"공격력 +{equip.humanAtkBonus}\n";
                if (equip.humanDefBonus != 0) desc += $"방어력 +{equip.humanDefBonus}\n";
                if (equip.humanSpdBonus != 0) desc += $"이동속도 +{equip.humanSpdBonus}\n";
            }

            // --- 메카 스탯 ---
            if (equip.mechaHpBonus != 0 || equip.mechaAtkBonus != 0 || equip.mechaDefBonus != 0 || equip.mechaSpdBonus != 0)
            {
                desc += "<color=#FFA07A>[메카]</color>\n";
                if (equip.mechaHpBonus != 0) desc += $"체력 +{equip.mechaHpBonus}\n";
                if (equip.mechaAtkBonus != 0) desc += $"공격력 +{equip.mechaAtkBonus}\n";
                if (equip.mechaDefBonus != 0) desc += $"방어력 +{equip.mechaDefBonus}\n";
                if (equip.mechaSpdBonus != 0) desc += $"이동속도 +{equip.mechaSpdBonus}\n";
            }

            // --- 드론 스탯 ---
            if (equip.droneAtkBonus != 0 || equip.droneFireRateBonus != 0 || equip.droneRangeBonus != 0)
            {
                desc += "<color=#98FB98>[드론]</color>\n";
                if (equip.droneAtkBonus != 0) desc += $"공격력 +{equip.droneAtkBonus}\n";
                if (equip.droneFireRateBonus != 0) desc += $"연사속도 +{equip.droneFireRateBonus}\n";
                if (equip.droneRangeBonus != 0) desc += $"인식거리 +{equip.droneRangeBonus}\n";
            }
        }

        descriptionText.text = desc;
    }

    public void Hide()
    {
        if (tooltipRoot != null) tooltipRoot.SetActive(false);
    }
}