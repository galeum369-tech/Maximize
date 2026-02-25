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
            iconImage.gameObject.SetActive(true); // 이것도 SetActive로 통일
            countText.text = slot.count > 1 ? slot.count.ToString() : "";
        }
        else
        {
            iconImage.gameObject.SetActive(false); // 빈 칸이면 아이콘 끄기
            countText.text = "";
        }
    }

    public void SetFocus(bool isFocused)
    {
        if (focusOutline != null)
        {
            // [수정된 부분] enabled 대신 gameObject.SetActive를 사용해서 확실하게 껐다 켬!
            focusOutline.gameObject.SetActive(isFocused);
        }
    }
}