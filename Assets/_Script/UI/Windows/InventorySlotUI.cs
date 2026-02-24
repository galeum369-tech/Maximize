using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;
    public Image focusOutline;

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

    public void SetFocus(bool isFocused)
    {
        if (focusOutline != null)
        {
            focusOutline.enabled = isFocused;
        }
    }
}