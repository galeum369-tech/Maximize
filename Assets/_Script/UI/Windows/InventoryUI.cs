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

    [Header("장비 슬롯 UI (오른쪽 판넬)")]
    public InventorySlotUI[] equipSlots = new InventorySlotUI[4];
    private ItemSlot[] tempEquipData = new ItemSlot[4] { new ItemSlot(), new ItemSlot(), new ItemSlot(), new ItemSlot() };

    [Header("조작 및 연출")]
    public Image floatingIcon;
    private int grabbedIndex = -1;
    private bool isWaitingForQuickSlot = false;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private int invFocusIndex = 0;   // 왼쪽(가방) 인덱스
    private int equipFocusIndex = 0; // 오른쪽(장비) 인덱스
    private int columns = 8;

    private int settingsFocusIndex = 0;
    private int settingsCount = 3;

    public bool isOpen = false;

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
            currentInput.OnSwitchZone -= ToggleZone; // [추가] 구독 해제
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
            currentInput.OnSwitchZone += ToggleZone; // [추가] 이벤트 구독!
        }
    }

    private void ChangeTabPrev() => ChangeTab(-1);
    private void ChangeTabNext() => ChangeTab(1);

    private void OnDestroy() => SetInputHandler(null);

    private void Update()
    {
        if (!isOpen) return;

        if (isWaitingForQuickSlot)
        {
            if (Input.GetKeyDown(KeyCode.Q)) AssignToQuickSlot(0);
            else if (Input.GetKeyDown(KeyCode.W)) AssignToQuickSlot(1);
            else if (Input.GetKeyDown(KeyCode.E)) AssignToQuickSlot(2);
            else if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            {
                isWaitingForQuickSlot = false;
                Debug.Log("퀵슬롯 등록 취소");
            }
            return;
        }

        if (grabbedIndex != -1 && floatingIcon != null && floatingIcon.gameObject.activeSelf)
        {
            Vector3 offset = new Vector3(20f, -20f, 0f);
            Transform targetTransform = currentZone == UIZone.Inventory ? uiSlots[invFocusIndex].transform : equipSlots[equipFocusIndex].transform;
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
            isWaitingForQuickSlot = false;
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

        // 탭이 바뀌면 무조건 기본 구역(인벤토리)으로 포커스 초기화
        currentZone = UIZone.Inventory;
        RefreshUI();
    }

    // ==========================================
    // [신규] X키를 눌렀을 때 구역(판넬) 전환 로직
    // ==========================================
    private void ToggleZone()
    {
        // 탭이 인벤토리가 아니거나, 아이템을 들고 있거나, 퀵슬롯 대기 중엔 구역 전환 불가
        if (!isOpen || currentTab != UITab.Inventory || grabbedIndex != -1 || isWaitingForQuickSlot) return;

        // 인벤토리 <-> 장비창 핑퐁 전환
        if (currentZone == UIZone.Inventory)
            currentZone = UIZone.Equipment;
        else
            currentZone = UIZone.Inventory;

        UpdateFocusVisuals();
        Debug.Log($"[UI] 포커스 판넬 전환: {currentZone}");
    }

    // ==========================================
    // [수정] 각 구역 내부에서만 커서가 돌도록 제한
    // ==========================================
    private void HandleNavigate(Vector2 dir)
    {
        if (!isOpen || isWaitingForQuickSlot) return;

        if (currentTab == UITab.Inventory)
        {
            if (currentZone == UIZone.Inventory)
            {
                // 인벤토리 40칸 안에서만 갇혀서 움직임
                if (dir.x > 0) invFocusIndex++;
                else if (dir.x < 0) invFocusIndex--;
                else if (dir.y > 0) invFocusIndex -= columns;
                else if (dir.y < 0) invFocusIndex += columns;

                invFocusIndex = Mathf.Clamp(invFocusIndex, 0, InventoryManager.Instance.maxSlotCount - 1);
            }
            else if (currentZone == UIZone.Equipment)
            {
                // [수정] 올려준 이미지를 보니 장비 4칸이 가로로 배치되어 있음!
                // 그래서 좌우 방향키(dir.x)로 포커스가 이동하도록 세팅함.
                if (dir.x > 0) equipFocusIndex++;
                else if (dir.x < 0) equipFocusIndex--;

                equipFocusIndex = Mathf.Clamp(equipFocusIndex, 0, 3);
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

    // 아이템 상호작용 (기존 코드와 동일)
    private void ExecuteInventoryAction()
    {
        if (isWaitingForQuickSlot) return;

        bool isMecha = PlayerTransformManager.Instance != null && PlayerTransformManager.Instance.IsMechaMode;

        if (currentZone == UIZone.Equipment)
        {
            if (grabbedIndex == -1)
            {
                EquipmentData[] currentEquips = isMecha ? InventoryManager.Instance.mechaEquips : InventoryManager.Instance.humanEquips;

                if (currentEquips[equipFocusIndex] != null)
                {
                    InventoryManager.Instance.UnequipItem(equipFocusIndex, isMecha);
                    RefreshUI();
                    if (statDisplay != null) statDisplay.RefreshStats();
                }
            }
            return;
        }

        var currentSlotData = InventoryManager.Instance.slots[invFocusIndex];

        if (grabbedIndex == -1)
        {
            if (currentSlotData.item != null)
            {
                grabbedIndex = invFocusIndex;
                floatingIcon.sprite = currentSlotData.item.icon;
                floatingIcon.gameObject.SetActive(true);
                uiSlots[grabbedIndex].iconImage.color = new Color(1, 1, 1, 0.5f);
            }
        }
        else
        {
            if (grabbedIndex == invFocusIndex)
            {
                DropGrabbedItem();

                if (currentSlotData.item.itemType == ItemType.Equipment)
                {
                    EquipmentData equip = currentSlotData.item as EquipmentData;
                    InventoryManager.Instance.EquipItem(equip, isMecha);

                    currentSlotData.count--;
                    if (currentSlotData.count <= 0) currentSlotData.Clear();
                }
                else if (currentSlotData.item.itemType == ItemType.Consumable)
                {
                    isWaitingForQuickSlot = true;
                    Debug.Log("퀵슬롯 단축키(Q,W,E)를 누르세요.");
                }
            }
            else
            {
                InventoryManager.Instance.MoveOrSwapSlot(grabbedIndex, invFocusIndex);
                DropGrabbedItem();
            }
        }

        RefreshUI();
        if (statDisplay != null) statDisplay.RefreshStats();
    }

    private void HandleSubmit()
    {
        if (!isOpen) return;

        if (currentTab == UITab.Inventory) ExecuteInventoryAction();
        else if (currentTab == UITab.Settings) ExecuteSettingsAction();
    }

    private void HandleCancel()
    {
        if (!isOpen) return;

        if (isWaitingForQuickSlot)
        {
            isWaitingForQuickSlot = false;
            return;
        }

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

    private void AssignToQuickSlot(int quickSlotIndex)
    {
        var slot = InventoryManager.Instance.slots[invFocusIndex];
        if (QuickSlotManager.Instance != null)
        {
            QuickSlotManager.Instance.RegisterSlot(quickSlotIndex, slot);
        }
        isWaitingForQuickSlot = false;
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

        if (PlayerTransformManager.Instance != null)
        {
            bool isMecha = PlayerTransformManager.Instance.IsMechaMode;
            EquipmentData[] currentEquips = isMecha ? InventoryManager.Instance.mechaEquips : InventoryManager.Instance.humanEquips;

            for (int i = 0; i < 4; i++)
            {
                tempEquipData[i].item = currentEquips[i];
                tempEquipData[i].count = currentEquips[i] != null ? 1 : 0;

                if (equipSlots[i] != null) equipSlots[i].UpdateSlot(tempEquipData[i]);
            }
        }

        UpdateFocusVisuals();
    }

    private void UpdateFocusVisuals()
    {
        foreach (var slot in uiSlots) slot.SetFocus(false);
        foreach (var slot in equipSlots) slot.SetFocus(false);

        ItemData focusedItem = null;

        // 선택된 구역(판넬)에만 테두리를 켜줌
        if (currentZone == UIZone.Inventory)
        {
            uiSlots[invFocusIndex].SetFocus(true);
            focusedItem = InventoryManager.Instance.slots[invFocusIndex].item;
        }
        else if (currentZone == UIZone.Equipment)
        {
            equipSlots[equipFocusIndex].SetFocus(true);
            focusedItem = tempEquipData[equipFocusIndex].item;
        }

        if (focusedItem != null && tooltip != null) tooltip.Show(focusedItem);
        else if (tooltip != null) tooltip.Hide();
    }
}