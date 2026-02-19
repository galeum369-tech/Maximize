using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image focusOutline; // 선택되었을 때 나타날 테두리 이미지 (Inspector에서 할당)

    public void UpdateSlot(ItemSlot slot)
    {
        if (slot != null && slot.item != null)
        {
            iconImage.sprite = slot.item.icon;
            iconImage.enabled = true;
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
        else
        {
            iconImage.enabled = false;
            countText.text = "";
        }
    }

    // 인벤토리 매니저가 호출해줄 포커스 시각화
    public void SetFocus(bool isFocused)
    {
        if (focusOutline != null)
        {
            focusOutline.enabled = isFocused;
        }
    }
}