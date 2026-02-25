using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    // [수정] 인스펙터 할당 삭제! 
    private PlayerInputHandler currentInput;

    [Header("퀵슬롯 데이터 (3개)")]
    public ItemSlot[] quickSlots = new ItemSlot[3];

    [Header("UI 요소")]
    public Image[] iconImages;
    public TextMeshProUGUI[] countTexts;

    private void Start()
    {
        Instance = this;
        for (int i = 0; i < 3; i++) quickSlots[i] = new ItemSlot();
    }

    // ==========================================
    // [핵심 추가] 폼 체인지 시 새로운 인풋 핸들러를 주입받음
    // ==========================================
    public void SetInputHandler(PlayerInputHandler newInput)
    {
        // 1. 기존 구독 해지
        if (currentInput != null)
        {
            currentInput.OnUseItem1 -= UseItem1;
            currentInput.OnUseItem2 -= UseItem2;
            currentInput.OnUseItem3 -= UseItem3;
        }

        currentInput = newInput;

        // 2. 새 구독 연결
        if (currentInput != null)
        {
            currentInput.OnUseItem1 += UseItem1;
            currentInput.OnUseItem2 += UseItem2;
            currentInput.OnUseItem3 += UseItem3;
        }
    }

    private void UseItem1() => UseItem(0);
    private void UseItem2() => UseItem(1);
    private void UseItem3() => UseItem(2);

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