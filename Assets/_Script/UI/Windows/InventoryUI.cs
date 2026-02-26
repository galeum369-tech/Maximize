using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UITab { Inventory, Settings }
public enum UIZone { Inventory, Equipment } // 나중에 Shop, Forge 등으로 확장 가능!

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("탭 & 구역 상태")]
    public GameObject inventoryPage;
    public GameObject settingsPage;
    public UITab currentTab = UITab.Inventory;
    public UIZone currentZone = UIZone.Inventory; // 현재 포커스가 있는 판넬

    private PlayerInputHandler currentInput;

    [Header("참조")]
    public GameObject inventoryRoot;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    [Header("장비/퀵슬롯 UI (오른쪽 판넬)")]
    public InventorySlotUI[] equipSlots = new InventorySlotUI[4];
    public InventorySlotUI[] quickSlotUIs = new InventorySlotUI[3]; // [추가] 장비창 쪽에 보여줄 퀵슬롯 UI
    private ItemSlot[] tempEquipData = new ItemSlot[4] { new ItemSlot(), new ItemSlot(), new ItemSlot(), new ItemSlot() };

    [Header("조작 및 연출")]
    public Image floatingIcon;
    private int grabbedIndex = -1;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private int invFocusIndex = 0;   // 왼쪽(가방) 인덱스

    // [수정] 오른쪽 판넬 인덱스 (0~3: 장비, 4~6: 퀵슬롯)
    private int rightFocusIndex = 0;
    private int columns = 8;

    private int settingsFocusIndex = 0;
    private int settingsCount = 3;

    public bool isOpen = false;

    [Header("더블클릭 감지용")]
    private float lastZPressTime = 0f;
    private float doubleClickThreshold = 0.3f; // 0.3초 이내 누르면 더블클릭

    private void Awake()
    {
        Instance = this;
        inventoryRoot.SetActive(false);
        if (floatingIcon != null) floatingIcon.gameObject.SetActive(false);
    }

    private void Start()
    {
        for (int i = 0; i < InventoryManager.Instance.maxSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            uiSlots.Add(go.GetComponent<InventorySlotUI>());
        }
    }

    public void SetInputHandler(PlayerInputHandler newInput)
    {
        if (currentInput != null)
        {
            currentInput.OnInventory -= ToggleUI;
            currentInput.OnCloseUI -= ToggleUI;
            currentInput.OnNavigate -= HandleNavigate;
            currentInput.OnSubmit -= HandleSubmit;
            currentInput.OnCancel -= HandleCancel;
            currentInput.OnPrevTab -= ChangeTabPrev;
            currentInput.OnNextTab -= ChangeTabNext;
            currentInput.OnSwitchZone -= ToggleZone;
        }

        currentInput = newInput;

        if (currentInput != null)
        {
            currentInput.OnInventory += ToggleUI;
            currentInput.OnCloseUI += ToggleUI;
            currentInput.OnNavigate += HandleNavigate;
            currentInput.OnSubmit += HandleSubmit;
            currentInput.OnCancel += HandleCancel;
            currentInput.OnPrevTab += ChangeTabPrev;
            currentInput.OnNextTab += ChangeTabNext;
            currentInput.OnSwitchZone += ToggleZone;
        }
    }

    private void ChangeTabPrev() => ChangeTab(-1);
    private void ChangeTabNext() => ChangeTab(1);

    private void OnDestroy() => SetInputHandler(null);

    private void Update()
    {
        if (!isOpen) return;

        // 들고 있는 아이콘 따라다니기 (오른쪽 존일 경우 알맞은 슬롯에 포커스)
        if (grabbedIndex != -1 && floatingIcon != null && floatingIcon.gameObject.activeSelf)
        {
            Vector3 offset = new Vector3(20f, -20f, 0f);
            Transform targetTransform;

            if (currentZone == UIZone.Inventory)
                targetTransform = uiSlots[invFocusIndex].transform;
            else if (rightFocusIndex < 4)
                targetTransform = equipSlots[rightFocusIndex].transform;
            else
                targetTransform = quickSlotUIs[rightFocusIndex - 4].transform;

            floatingIcon.transform.position = targetTransform.position + offset;
        }
    }

    private void ToggleUI()
    {
        isOpen = !isOpen;
        inventoryRoot.SetActive(isOpen);

        if (isOpen)
        {
            RefreshUI();
            if (statDisplay != null) statDisplay.RefreshStats();
            if (currentInput != null) currentInput.OpenUI(true);
        }
        else
        {
            DropGrabbedItem();
            if (tooltip != null) tooltip.Hide();
            if (currentInput != null) currentInput.OpenUI(false);
        }
    }

    private void CloseUI()
    {
        if (isOpen) ToggleUI();
    }

    private void ChangeTab(int dir)
    {
        if (!isOpen || grabbedIndex != -1) return;

        int tabCount = System.Enum.GetValues(typeof(UITab)).Length;
        int nextTab = ((int)currentTab + dir + tabCount) % tabCount;
        currentTab = (UITab)nextTab;

        inventoryPage.SetActive(currentTab == UITab.Inventory);
        settingsPage.SetActive(currentTab == UITab.Settings);

        currentZone = UIZone.Inventory;
        RefreshUI();
    }

    // 아이템을 쥐고 있어도 구역 이동 가능
    private void ToggleZone()
    {
        if (!isOpen || currentTab != UITab.Inventory) return;

        currentZone = (currentZone == UIZone.Inventory) ? UIZone.Equipment : UIZone.Inventory;

        UpdateFocusVisuals();
        Debug.Log($"[UI] 포커스 판넬 전환: {currentZone}");
    }

    // 우측 판넬(장비4 + 퀵슬롯3) 네비게이션 처리
    private void HandleNavigate(Vector2 dir)
    {
        if (!isOpen) return;

        if (currentTab == UITab.Inventory)
        {
            if (currentZone == UIZone.Inventory)
            {
                if (dir.x > 0) invFocusIndex++;
                else if (dir.x < 0) invFocusIndex--;
                else if (dir.y > 0) invFocusIndex -= columns;
                else if (dir.y < 0) invFocusIndex += columns;

                invFocusIndex = Mathf.Clamp(invFocusIndex, 0, InventoryManager.Instance.maxSlotCount - 1);
            }
            else if (currentZone == UIZone.Equipment)
            {
                // 좌우 이동
                if (dir.x > 0) rightFocusIndex++;
                else if (dir.x < 0) rightFocusIndex--;

                // 상하 이동 (장비 <-> 퀵슬롯 간 이동)
                else if (dir.y < 0) // 아래로
                {
                    if (rightFocusIndex < 4) rightFocusIndex = Mathf.Min(rightFocusIndex + 4, 6);
                }
                else if (dir.y > 0) // 위로
                {
                    if (rightFocusIndex >= 4) rightFocusIndex -= 4;
                }

                rightFocusIndex = Mathf.Clamp(rightFocusIndex, 0, 6);
            }
            UpdateFocusVisuals();
        }
        else if (currentTab == UITab.Settings)
        {
            if (dir.y > 0) settingsFocusIndex--;
            else if (dir.y < 0) settingsFocusIndex++;
            settingsFocusIndex = Mathf.Clamp(settingsFocusIndex, 0, settingsCount - 1);
        }
    }

    private void HandleSubmit()
    {
        if (!isOpen) return;

        if (currentTab == UITab.Inventory)
        {
            bool isDoubleClick = (Time.time - lastZPressTime) <= doubleClickThreshold;
            lastZPressTime = Time.time;

            ExecuteInventoryAction(isDoubleClick);
        }
        else if (currentTab == UITab.Settings) ExecuteSettingsAction();
    }

    // [핵심 로직] 장비와 마찬가지로 소모품도 '물리적 이동'으로 처리
    private void ExecuteInventoryAction(bool isDoubleClick)
    {
        // 1. 우측 판넬 (장비 or 퀵슬롯) 포커스 시
        if (currentZone == UIZone.Equipment)
        {
            if (grabbedIndex == -1) // 빈 손 (장착 해제)
            {
                if (rightFocusIndex < 4) // 장비칸
                {
                    InventoryManager.Instance.UnequipItem(rightFocusIndex);
                }
                else // 퀵슬롯 (4, 5, 6)
                {
                    int quickIndex = rightFocusIndex - 4;
                    // [수정] Clear가 아니라 반환 로직(Unequip)을 호출!
                    QuickSlotManager.Instance.UnequipQuickSlot(quickIndex);
                }
            }
            else // 아이템 들고 있는 상태 (수동 장착 시도)
            {
                var grabbedSlot = InventoryManager.Instance.slots[grabbedIndex];

                if (rightFocusIndex < 4) // 장비칸에 올려놓기
                {
                    if (grabbedSlot.item.itemType == ItemType.Equipment)
                    {
                        EquipmentData equip = grabbedSlot.item as EquipmentData;
                        if ((int)equip.equipType == rightFocusIndex)
                        {
                            InventoryManager.Instance.EquipItem(equip);
                            grabbedSlot.count--;
                            if (grabbedSlot.count <= 0) grabbedSlot.Clear();
                            DropGrabbedItem();
                        }
                        else Debug.Log("그 부위에는 장착할 수 없어!");
                    }
                    else Debug.Log("장비칸에는 장비만 올릴 수 있어!");
                }
                else // 퀵슬롯에 올려놓기
                {
                    if (grabbedSlot.item.itemType == ItemType.Consumable)
                    {
                        int quickIndex = rightFocusIndex - 4;
                        // [수정] 수동 장착 시 퀵슬롯으로 물리적 이동 처리
                        QuickSlotManager.Instance.EquipToQuickSlot(quickIndex, grabbedSlot);
                        DropGrabbedItem();
                    }
                    else Debug.Log("퀵슬롯에는 소모품만 등록 가능해!");
                }
            }
            RefreshUI();
            if (statDisplay != null) statDisplay.RefreshStats();
            return;
        }

        // 2. 좌측 판넬 (인벤토리) 포커스 시
        var currentSlotData = InventoryManager.Instance.slots[invFocusIndex];

        if (grabbedIndex == -1) // 손이 비어있음
        {
            if (currentSlotData.item != null)
            {
                if (isDoubleClick) // 자동 장착/등록
                {
                    if (currentSlotData.item.itemType == ItemType.Equipment)
                    {
                        EquipmentData equip = currentSlotData.item as EquipmentData;
                        InventoryManager.Instance.EquipItem(equip);
                        currentSlotData.count--;
                        if (currentSlotData.count <= 0) currentSlotData.Clear();
                        Debug.Log($"{equip.itemName} 자동 장착 완료!");
                    }
                    else if (currentSlotData.item.itemType == ItemType.Consumable)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (QuickSlotManager.Instance.quickSlots[i].item == null)
                            {
                                // [수정] 자동 장착 시 퀵슬롯으로 물리적 이동 처리
                                QuickSlotManager.Instance.EquipToQuickSlot(i, currentSlotData);
                                Debug.Log($"퀵슬롯 {i + 1}번에 자동 등록 완료!");
                                break;
                            }
                        }
                    }
                }
                else // 집기
                {
                    grabbedIndex = invFocusIndex;
                    floatingIcon.sprite = currentSlotData.item.icon;
                    floatingIcon.gameObject.SetActive(true);
                    uiSlots[grabbedIndex].iconImage.color = new Color(1, 1, 1, 0.5f);
                }
            }
        }
        else // 아이템 들고 있음 (가방 안에 내려놓거나 스왑)
        {
            if (grabbedIndex == invFocusIndex) DropGrabbedItem();
            else
            {
                InventoryManager.Instance.MoveOrSwapSlot(grabbedIndex, invFocusIndex);
                DropGrabbedItem();
            }
        }

        RefreshUI();
        if (statDisplay != null) statDisplay.RefreshStats();
    }

    private void HandleCancel()
    {
        if (!isOpen) return;

        if (grabbedIndex != -1)
        {
            DropGrabbedItem();
            return;
        }

        CloseUI();
    }

    private void DropGrabbedItem()
    {
        if (grabbedIndex != -1)
        {
            uiSlots[grabbedIndex].iconImage.color = Color.white;
            floatingIcon.gameObject.SetActive(false);
            grabbedIndex = -1;
        }
    }

    private void ExecuteSettingsAction()
    {
        switch (settingsFocusIndex)
        {
            case 0: Debug.Log("마을로 돌아가기"); break;
            case 1: Debug.Log("타이틀로 이동"); break;
            case 2: Debug.Log("게임 종료"); Application.Quit(); break;
        }
    }

    public void RefreshUI()
    {
        var dataSlots = InventoryManager.Instance.slots;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].UpdateSlot(dataSlots[i]);

            if (i == grabbedIndex) uiSlots[i].iconImage.color = new Color(1, 1, 1, 0.5f);
            else uiSlots[i].iconImage.color = Color.white;
        }

        // [수정] isMecha 검사할 필요 없이 그냥 공통 장비 배열(equippedItems)을 가져와서 그리면 끝!
        EquipmentData[] currentEquips = InventoryManager.Instance.equippedItems;

        for (int i = 0; i < 4; i++)
        {
            tempEquipData[i].item = currentEquips[i];
            tempEquipData[i].count = currentEquips[i] != null ? 1 : 0;
            if (equipSlots[i] != null) equipSlots[i].UpdateSlot(tempEquipData[i]);
        }

        // 퀵슬롯 UI 갱신 (인벤토리 판넬 안의 퀵슬롯 보여주기용)
        if (QuickSlotManager.Instance != null)
        {
            for (int i = 0; i < 3; i++)
            {
                if (quickSlotUIs[i] != null) quickSlotUIs[i].UpdateSlot(QuickSlotManager.Instance.quickSlots[i]);
            }
        }

        UpdateFocusVisuals();
    }

    private void UpdateFocusVisuals()
    {
        foreach (var slot in uiSlots) slot.SetFocus(false);
        foreach (var slot in equipSlots) slot.SetFocus(false);
        foreach (var slot in quickSlotUIs) slot.SetFocus(false);

        ItemData focusedItem = null;

        if (currentZone == UIZone.Inventory)
        {
            uiSlots[invFocusIndex].SetFocus(true);
            focusedItem = InventoryManager.Instance.slots[invFocusIndex].item;
        }
        else if (currentZone == UIZone.Equipment)
        {
            if (rightFocusIndex < 4)
            {
                equipSlots[rightFocusIndex].SetFocus(true);
                focusedItem = tempEquipData[rightFocusIndex].item;
            }
            else
            {
                int quickIndex = rightFocusIndex - 4;
                quickSlotUIs[quickIndex].SetFocus(true);
                focusedItem = QuickSlotManager.Instance.quickSlots[quickIndex].item;
            }
        }

        if (focusedItem != null && tooltip != null) tooltip.Show(focusedItem);
        else if (tooltip != null) tooltip.Hide();
    }
}