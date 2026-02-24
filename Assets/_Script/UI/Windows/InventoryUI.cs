using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UITab { Inventory, Settings }

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("탭 페이지 참조")]
    public GameObject inventoryPage;
    public GameObject settingsPage;
    public UITab currentTab = UITab.Inventory;

    [Header("참조")]
    public PlayerInputHandler inputHandler;
    public GameObject inventoryRoot;
    public Transform slotParent; // 우측 40칸 그리드 부모
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    [Header("장비 슬롯 UI (왼쪽)")]
    // 0:Core, 1:Frame, 2:Gear, 3:Chip 순서대로 할당
    public InventorySlotUI[] equipSlots = new InventorySlotUI[4];
    private ItemSlot[] tempEquipData = new ItemSlot[4] { new ItemSlot(), new ItemSlot(), new ItemSlot(), new ItemSlot() };

    [Header("조작 및 연출")]
    public Image floatingIcon; // 집어 든 가짜 아이콘
    private int grabbedIndex = -1; // 현재 쥐고 있는 가방 슬롯 번호
    private bool isWaitingForQuickSlot = false; // 퀵슬롯 입력 대기

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>(); // 가방 40칸 UI
    private int currentFocusIndex = 0;
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
        // 인벤토리 40칸 생성
        for (int i = 0; i < InventoryManager.Instance.maxSlotCount; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            uiSlots.Add(go.GetComponent<InventorySlotUI>());
        }

        if (inputHandler != null)
        {
            inputHandler.OnInventory += ToggleUI;
            inputHandler.OnNavigate += HandleNavigate;
            inputHandler.OnSubmit += HandleSubmit;
            inputHandler.OnCancel += HandleCancel;
            inputHandler.OnPrevTab += () => ChangeTab(-1);
            inputHandler.OnNextTab += () => ChangeTab(1);
        }
    }

    private void OnDestroy()
    {
        if (inputHandler != null)
        {
            inputHandler.OnInventory -= ToggleUI;
            inputHandler.OnNavigate -= HandleNavigate;
            inputHandler.OnSubmit -= HandleSubmit;
            inputHandler.OnCancel -= HandleCancel;
            inputHandler.OnPrevTab -= () => ChangeTab(-1);
            inputHandler.OnNextTab -= () => ChangeTab(1);
        }
    }

    private void Update()
    {
        if (!isOpen) return;

        // 퀵슬롯 대기 중 단축키 감지
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

        // 떠다니는 아이콘 위치 갱신 (포커스 우측 하단)
        if (grabbedIndex != -1 && floatingIcon != null && floatingIcon.gameObject.activeSelf)
        {
            Vector3 offset = new Vector3(20f, -20f, 0f);
            floatingIcon.transform.position = uiSlots[currentFocusIndex].transform.position + offset;
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
        }
        else
        {
            DropGrabbedItem();
            isWaitingForQuickSlot = false;
            if (tooltip != null) tooltip.Hide();
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

        RefreshUI();
    }

    private void HandleNavigate(Vector2 dir)
    {
        if (!isOpen || isWaitingForQuickSlot) return;

        if (currentTab == UITab.Inventory)
        {
            if (dir.x > 0) currentFocusIndex++;
            else if (dir.x < 0) currentFocusIndex--;
            else if (dir.y > 0) currentFocusIndex -= columns;
            else if (dir.y < 0) currentFocusIndex += columns;

            currentFocusIndex = Mathf.Clamp(currentFocusIndex, 0, InventoryManager.Instance.maxSlotCount - 1);
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

        if (currentTab == UITab.Inventory) ExecuteInventoryAction();
        else if (currentTab == UITab.Settings) ExecuteSettingsAction();
    }

    // 아이템 상호작용 (집기, 장착, 교체)
    private void ExecuteInventoryAction()
    {
        if (isWaitingForQuickSlot) return;

        var currentSlotData = InventoryManager.Instance.slots[currentFocusIndex];

        // 1. 빈손일 때 집어들기
        if (grabbedIndex == -1)
        {
            if (currentSlotData.item != null)
            {
                grabbedIndex = currentFocusIndex;
                floatingIcon.sprite = currentSlotData.item.icon;
                floatingIcon.gameObject.SetActive(true);
                uiSlots[grabbedIndex].iconImage.color = new Color(1, 1, 1, 0.5f); // 원래 위치 반투명
            }
        }
        // 2. 아이템을 들고 있을 때
        else
        {
            // A. 제자리 더블클릭 (장착 혹은 사용)
            if (grabbedIndex == currentFocusIndex)
            {
                DropGrabbedItem();

                if (currentSlotData.item.itemType == ItemType.Equipment)
                {
                    EquipmentData equip = currentSlotData.item as EquipmentData;
                    bool isMecha = PlayerTransformManager.Instance != null && PlayerTransformManager.Instance.IsMechaMode;

                    // 매니저를 통해 장착 처리
                    InventoryManager.Instance.EquipItem(equip, isMecha);

                    // 가방에서 1개 소모 (장착됨)
                    currentSlotData.count--;
                    if (currentSlotData.count <= 0) currentSlotData.Clear();

                    Debug.Log($"{equip.itemName} 장착 완료!");
                }
                else if (currentSlotData.item.itemType == ItemType.Consumable)
                {
                    isWaitingForQuickSlot = true;
                    Debug.Log("퀵슬롯 단축키(Q,W,E)를 누르세요.");
                }
            }
            // B. 다른 자리에 내려놓음 (이동/병합)
            else
            {
                InventoryManager.Instance.MoveOrSwapSlot(grabbedIndex, currentFocusIndex);
                DropGrabbedItem();
            }
        }

        RefreshUI();
        if (statDisplay != null) statDisplay.RefreshStats();
    }

    private void HandleCancel()
    {
        if (!isOpen) return;

        if (isWaitingForQuickSlot)
        {
            isWaitingForQuickSlot = false;
            return;
        }

        // 아이템 들고 있으면 취소
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
        var slot = InventoryManager.Instance.slots[currentFocusIndex];
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

    // 전체 UI 갱신 (가방 + 장비창)
    public void RefreshUI()
    {
        // 1. 가방 40칸 갱신
        var dataSlots = InventoryManager.Instance.slots;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].UpdateSlot(dataSlots[i]);

            if (i == grabbedIndex) uiSlots[i].iconImage.color = new Color(1, 1, 1, 0.5f);
            else uiSlots[i].iconImage.color = Color.white;
        }

        // 2. 장착된 장비 4칸 갱신
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
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].SetFocus(i == currentFocusIndex);
        }

        var item = InventoryManager.Instance.slots[currentFocusIndex].item;
        if (item != null && tooltip != null) tooltip.Show(item);
        else if (tooltip != null) tooltip.Hide();
    }
}