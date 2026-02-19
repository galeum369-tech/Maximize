using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemTooltipUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI rarityText;
    public TextMeshProUGUI descriptionText;
    public GameObject tooltipRoot; // 툴팁 부모 오브젝트

    public void Show(ItemData item)
    {
        tooltipRoot.SetActive(true);

        // 이름과 등급 텍스트 설정
        itemNameText.text = item.itemName;
        rarityText.text = UIUtils.GetRarityName(item.rarity);

        // 등급에 맞는 색상 적용
        Color rarityColor = UIUtils.GetRarityColor(item.rarity);
        itemNameText.color = rarityColor;
        rarityText.color = rarityColor;

        // 설명글 구성 (장비라면 스탯 정보 추가)
        string desc = "";
        if (item is EquipmentData equip)
        {
            if (equip.atkBonus != 0) desc += $"공격력 +{equip.atkBonus}\n";
            if (equip.hpBonus != 0) desc += $"체력 +{equip.hpBonus}\n";
            if (equip.defBonus != 0) desc += $"방어력 +{equip.defBonus}%\n"; // 방어력은 % 느낌으로 표시
            if (equip.spdBonus != 0) desc += $"이동속도 +{equip.spdBonus}\n";
        }
        // item.description 필드가 있다면 추가 가능
        descriptionText.text = desc;
    }

    public void Hide() => tooltipRoot.SetActive(false);
}