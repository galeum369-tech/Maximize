using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    private PlayerInputHandler currentInput;

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

    public void SetInputHandler(PlayerInputHandler newInput)
    {
        if (currentInput != null)
        {
            currentInput.OnUseItem1 -= UseItem1;
            currentInput.OnUseItem2 -= UseItem2;
            currentInput.OnUseItem3 -= UseItem3;
        }

        currentInput = newInput;

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

    private void UseItem(int index)
    {
        var slot = quickSlots[index];
        if (slot.item != null && slot.item is ConsumableData consumable)
        {
            Debug.Log($"{consumable.itemName} 사용! 체력 {consumable.healAmount} 회복!");

            if (PlayerTransformManager.Instance.IsMechaMode)
                PlayerTransformManager.Instance.mechaObject.GetComponent<MechaState>().currentHp += consumable.healAmount;
            else
                PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>().currentHp += consumable.healAmount;

            slot.count--;
            if (slot.count <= 0) slot.Clear(); // 다 쓰면 빈칸으로 만들기

            RefreshQuickSlotUI();

            if (InventoryUI.Instance != null && InventoryUI.Instance.isOpen)
                InventoryUI.Instance.RefreshUI();
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
                countTexts[i].text = quickSlots[i].count > 1 ? quickSlots[i].count.ToString() : "";
            }
            else
            {
                iconImages[i].enabled = false;
                countTexts[i].text = "";
            }
        }
    }

    // [핵심 1] 인벤토리에서 아이템을 가져와서 퀵슬롯에 '물리적'으로 장착 (이동/스왑)
    public void EquipToQuickSlot(int index, ItemSlot bagSlot)
    {
        if (quickSlots[index].item != null)
        {
            // 이미 퀵슬롯에 뭐가 있다면 가방 아이템과 스왑 (자리 바꾸기)
            ItemSlot temp = new ItemSlot();
            temp.item = quickSlots[index].item;
            temp.count = quickSlots[index].count;

            quickSlots[index].item = bagSlot.item;
            quickSlots[index].count = bagSlot.count;

            bagSlot.item = temp.item;
            bagSlot.count = temp.count;
        }
        else
        {
            // 비어있으면 쏙 넣고 가방칸을 완전히 비움 (장비랑 똑같이!)
            quickSlots[index].item = bagSlot.item;
            quickSlots[index].count = bagSlot.count;
            bagSlot.Clear();
        }
        RefreshQuickSlotUI();
    }

    // [핵심 2] 퀵슬롯 해제 시 가방으로 안전하게 반환
    public void UnequipQuickSlot(int index)
    {
        if (quickSlots[index].item != null)
        {
            // 가방에 AddItem을 통해 반환을 시도
            bool success = InventoryManager.Instance.AddItem(quickSlots[index].item, quickSlots[index].count);
            if (success)
            {
                quickSlots[index].Clear(); // 가방에 들어갔을 때만 퀵슬롯 지우기
                RefreshQuickSlotUI();
                Debug.Log($"퀵슬롯 {index + 1}번 해제! 인벤토리로 돌아감.");
            }
            else
            {
                Debug.Log("가방이 꽉 차서 퀵슬롯을 해제할 수 없어!");
            }
        }
    }

    // [핵심 3] 땅에서 아이템을 먹었을 때 퀵슬롯에 같은 게 있으면 가방보다 '먼저' 채워주기
    public int AddToQuickSlotFirst(ItemData data, int amount)
    {
        for (int i = 0; i < 3; i++)
        {
            if (quickSlots[i].item == data && quickSlots[i].count < data.GetMaxStack())
            {
                int canAdd = data.GetMaxStack() - quickSlots[i].count;
                int toAdd = Mathf.Min(canAdd, amount);
                quickSlots[i].count += toAdd;
                amount -= toAdd;

                RefreshQuickSlotUI();
                if (amount <= 0) break; // 다 채웠으면 종료
            }
        }
        return amount; // 남은 갯수를 리턴 (이 남은 갯수가 가방으로 들어감)
    }
}