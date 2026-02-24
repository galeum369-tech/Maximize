using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    [Header("참조")]
    public PlayerInputHandler inputHandler;

    [Header("퀵슬롯 데이터 (3개)")]
    public ItemSlot[] quickSlots = new ItemSlot[3];

    [Header("UI 요소")]
    public Image[] iconImages;
    public TextMeshProUGUI[] countTexts;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < 3; i++) quickSlots[i] = new ItemSlot();
    }

    private void OnEnable()
    {
        inputHandler.OnUseItem1 += () => UseItem(0);
        inputHandler.OnUseItem2 += () => UseItem(1);
        inputHandler.OnUseItem3 += () => UseItem(2);
    }

    public void UseItem(int index)
    {
        ItemSlot slot = quickSlots[index];

        if (slot.item != null && slot.count > 0)
        {
            if (slot.item.itemType == ItemType.Consumable)
            {
                ConsumableData consumable = slot.item as ConsumableData;
                Debug.Log($"{consumable.itemName} 사용! 체력 {consumable.healAmount} 회복!");

                if (PlayerTransformManager.Instance.IsMechaMode)
                    PlayerTransformManager.Instance.mechaObject.GetComponent<MechaState>().currentHp += consumable.healAmount;
                else
                    PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>().currentHp += consumable.healAmount;

                slot.count--;
                if (slot.count <= 0) slot.Clear();

                RefreshQuickSlotUI();

                if (InventoryUI.Instance != null && InventoryUI.Instance.isOpen)
                    InventoryUI.Instance.RefreshUI();
            }
        }
    }

    public void RefreshQuickSlotUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (quickSlots[i].item != null && quickSlots[i].count > 0)
            {
                iconImages[i].sprite = quickSlots[i].item.icon;
                iconImages[i].enabled = true;
                countTexts[i].text = quickSlots[i].count.ToString();
            }
            else
            {
                iconImages[i].enabled = false;
                countTexts[i].text = "";
            }
        }
    }

    public void RegisterSlot(int slotIndex, ItemSlot inventorySlot)
    {
        quickSlots[slotIndex] = inventorySlot;
        RefreshQuickSlotUI();
        Debug.Log($"퀵슬롯 {slotIndex + 1}번에 {inventorySlot.item.itemName} 등록 완료!");
    }
}