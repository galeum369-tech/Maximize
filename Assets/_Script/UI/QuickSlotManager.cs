using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuickSlotManager : MonoBehaviour
{
    public static QuickSlotManager Instance { get; private set; }

    [Header("참조")]
    public PlayerInputHandler inputHandler;

    [Header("퀵슬롯 데이터 (3개)")]
    public ItemSlot[] quickSlots = new ItemSlot[3]; // 실제 데이터

    [Header("UI 요소")]
    public Image[] iconImages;     // 아이콘 이미지 3개
    public TextMeshProUGUI[] countTexts; // 개수 텍스트 3개

    private void Awake()
    {
        Instance = this;
        // 초기 데이터 슬롯 생성 (비어있음)
        for (int i = 0; i < 3; i++) quickSlots[i] = new ItemSlot();
    }

    private void OnEnable()
    {
        // 핸들러 이벤트 연결
        inputHandler.OnUseItem1 += () => UseItem(0);
        inputHandler.OnUseItem2 += () => UseItem(1);
        inputHandler.OnUseItem3 += () => UseItem(2);
    }

    // 아이템 사용 로직
    public void UseItem(int index)
    {
        ItemSlot slot = quickSlots[index];

        if (slot.item != null && slot.count > 0)
        {
            // 1. 소모품(Consumable) 타입인지 확인
            if (slot.item.itemType == ItemType.Consumable)
            {
                Debug.Log($"{slot.item.itemName} 사용!");

                // 2. 효과 적용 (예: 체력 회복 등 - 아이템 데이터에 로직 필요)
                // playerState.Heal(slot.item.healAmount);

                // 3. 개수 감소
                slot.count--;
                if (slot.count <= 0) slot.Clear();

                RefreshQuickSlotUI();
            }
        }
    }

    // UI 갱신
    public void RefreshQuickSlotUI()
    {
        for (int i = 0; i < 3; i++)
        {
            if (quickSlots[i].item != null)
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
}