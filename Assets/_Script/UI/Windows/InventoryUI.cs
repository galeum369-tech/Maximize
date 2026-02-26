using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum UITab { Inventory, Settings }
public enum UIZone { Inventory, Equipment, Storage, Shop } // [추가] Storage 구역 추가

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("탭 & 구역 상태")]
    public GameObject inventoryPage;
    public GameObject settingsPage;
    public UITab currentTab = UITab.Inventory;
    public UIZone currentZone = UIZone.Inventory;

    private PlayerInputHandler currentInput;

    [Header("참조")]
    public GameObject inventoryRoot;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    // ==========================================
    // [추가] 모드 스위칭용 부모 오브젝트 연결
    // ==========================================
    [Header("판넬 스위칭 (오른쪽 영역)")]
    public GameObject equipmentPanelRoot; // 장비+퀵슬롯 묶음
    public GameObject storagePanelRoot;   // 창고 스크롤뷰 묶음
    public bool isStorageMode = false;    // 현재 창고를 열었는지 여부
    [Header("판넬 스위칭 (상점 모드)")]
    public GameObject shopPanelRoot;   // 상점 스크롤뷰 묶음
    public bool isShopMode = false;    // 현재 상점을 열었는지 여부

    [Header("상점 UI")]
    public Transform shopSlotParent;
    private List<InventorySlotUI> shopUiSlots = new List<InventorySlotUI>();
    private int shopFocusIndex = 0;
    private int shopColumns = 5; // 창고랑 똑같이 세팅

    [Header("장비/퀵슬롯 UI (오른쪽 판넬)")]
    public InventorySlotUI[] equipSlots = new InventorySlotUI[4];
    public InventorySlotUI[] quickSlotUIs = new InventorySlotUI[3];
    private ItemSlot[] tempEquipData = new ItemSlot[4] { new ItemSlot(), new ItemSlot(), new ItemSlot(), new ItemSlot() };

    // ==========================================
    // [추가] 창고 슬롯 UI 리스트
    // ==========================================
    [Header("창고 UI")]
    public Transform storageSlotParent; // Grid Layout Group이 있는 부모
    private List<InventorySlotUI> storageUiSlots = new List<InventorySlotUI>();
    private int storageFocusIndex = 0;
    private int storageColumns = 5; // 창고 가로 칸 수 (UI 세팅에 맞게 조절해)

    [Header("조작 및 연출")]
    public Image floatingIcon;
    private int grabbedIndex = -1;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();
    private int invFocusIndex = 0;
    private int rightFocusIndex = 0;
    private int columns = 8;

    private int settingsFocusIndex = 0;
    private int settingsCount = 3;

    public bool isOpen = false;

    private float lastZPressTime = 0f;
    private float doubleClickThreshold = 0.3f;

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

        if (grabbedIndex != -1 && floatingIcon != null && floatingIcon.gameObject.activeSelf)
        {
            Vector3 offset = new Vector3(20f, -20f, 0f);
            Transform targetTransform;

            if (currentZone == UIZone.Inventory)
                targetTransform = uiSlots[invFocusIndex].transform;
            else if (currentZone == UIZone.Equipment)
            {
                if (rightFocusIndex < 4) targetTransform = equipSlots[rightFocusIndex].transform;
                else targetTransform = quickSlotUIs[rightFocusIndex - 4].transform;
            }
            else // Storage Zone
            {
                if (storageUiSlots.Count > storageFocusIndex)
                    targetTransform = storageUiSlots[storageFocusIndex].transform;
                else targetTransform = storageSlotParent; // 안전빵
            }

            floatingIcon.transform.position = targetTransform.position + offset;
        }
    }

    // ==========================================
    // [추가] 창고 상호작용 시 호출되는 전용 오픈 함수
    // ==========================================
    public void OpenStorageUI()
    {
        isStorageMode = true;
        currentZone = UIZone.Inventory; // 시작 포커스는 무조건 왼쪽 가방
        grabbedIndex = -1; // 혹시 쥐고 있던 거 초기화

        // 판넬 스위칭
        if (equipmentPanelRoot != null) equipmentPanelRoot.SetActive(false);
        if (storagePanelRoot != null) storagePanelRoot.SetActive(true);

        if (!isOpen) ToggleUI();
        else RefreshUI();
    }

    public void OpenShopUI()
    {
        isShopMode = true;
        currentZone = UIZone.Inventory;
        grabbedIndex = -1;

        if (equipmentPanelRoot != null) equipmentPanelRoot.SetActive(false);
        if (storagePanelRoot != null) storagePanelRoot.SetActive(false);
        if (shopPanelRoot != null) shopPanelRoot.SetActive(true);

        if (!isOpen) ToggleUI();
        else RefreshUI();
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

            // [추가] 닫을 때는 창고 모드 해제하고 장비창으로 원상복구
            isStorageMode = false;
            if (equipmentPanelRoot != null) equipmentPanelRoot.SetActive(true);
            if (storagePanelRoot != null) storagePanelRoot.SetActive(false);
            currentZone = UIZone.Inventory;

            isShopMode = false;
            if (shopPanelRoot != null) shopPanelRoot.SetActive(false);

            // ToggleZone() 내부:
            if (isStorageMode) currentZone = (currentZone == UIZone.Inventory) ? UIZone.Storage : UIZone.Inventory;
            else if (isShopMode) currentZone = (currentZone == UIZone.Inventory) ? UIZone.Shop : UIZone.Inventory; // 이거 추가!
            else currentZone = (currentZone == UIZone.Inventory) ? UIZone.Equipment : UIZone.Inventory;
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

    private void ToggleZone()
    {
        if (!isOpen || currentTab != UITab.Inventory) return;

        // [수정] 창고 모드일 때는 Inventory <-> Storage 로 스위칭
        if (isStorageMode)
        {
            currentZone = (currentZone == UIZone.Inventory) ? UIZone.Storage : UIZone.Inventory;
        }
        else
        {
            currentZone = (currentZone == UIZone.Inventory) ? UIZone.Equipment : UIZone.Inventory;
        }

        UpdateFocusVisuals();
    }

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
                if (dir.x > 0) rightFocusIndex++;
                else if (dir.x < 0) rightFocusIndex--;
                else if (dir.y < 0) { if (rightFocusIndex < 4) rightFocusIndex = Mathf.Min(rightFocusIndex + 4, 6); }
                else if (dir.y > 0) { if (rightFocusIndex >= 4) rightFocusIndex -= 4; }

                rightFocusIndex = Mathf.Clamp(rightFocusIndex, 0, 6);
            }
            // ==========================================
            // [추가] 창고 목록 포커스 이동
            // ==========================================
            else if (currentZone == UIZone.Storage)
            {
                int maxStorage = StorageManager.Instance.storageSlots.Count;
                if (maxStorage > 0)
                {
                    if (dir.x > 0) storageFocusIndex++;
                    else if (dir.x < 0) storageFocusIndex--;
                    else if (dir.y > 0) storageFocusIndex -= storageColumns;
                    else if (dir.y < 0) storageFocusIndex += storageColumns;

                    storageFocusIndex = Mathf.Clamp(storageFocusIndex, 0, maxStorage - 1);
                }
            }
            else if (currentZone == UIZone.Shop)
            {
                int maxShop = ShopManager.Instance.shopItems.Count;
                if (maxShop > 0)
                {
                    if (dir.x > 0) shopFocusIndex++;
                    else if (dir.x < 0) shopFocusIndex--;
                    else if (dir.y > 0) shopFocusIndex -= shopColumns;
                    else if (dir.y < 0) shopFocusIndex += shopColumns;

                    shopFocusIndex = Mathf.Clamp(shopFocusIndex, 0, maxShop - 1);
                }
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
            // ==========================================
            // [추가] 창고 모드일 때는 들고 옮기는 로직 대신 '빠른 보관/출금' 사용
            // ==========================================
            if (isStorageMode) ExecuteStorageAction();
            else if (isShopMode) ExecuteShopAction();
            else
            {
                bool isDoubleClick = (Time.time - lastZPressTime) <= doubleClickThreshold;
                lastZPressTime = Time.time;
                ExecuteInventoryAction(isDoubleClick);
            }
        }
        else if (currentTab == UITab.Settings) ExecuteSettingsAction();
    }

    // ==========================================
    // [추가] 창고 빠른 보관/출금 로직
    // ==========================================
    private void ExecuteStorageAction()
    {
        if (currentZone == UIZone.Inventory)
        {
            // 인벤 -> 창고로 넣기
            var invSlot = InventoryManager.Instance.slots[invFocusIndex];
            if (invSlot.item != null)
            {
                StorageManager.Instance.DepositItem(invFocusIndex);
                RefreshUI();
            }
        }
        else if (currentZone == UIZone.Storage)
        {
            // 창고 -> 인벤으로 빼기
            if (StorageManager.Instance.storageSlots.Count > storageFocusIndex)
            {
                StorageManager.Instance.WithdrawItem(storageFocusIndex);
                // 빼고 나서 리스트가 줄어들었을 때 포커스가 오바되지 않게 잡아줌
                int maxStorage = Mathf.Max(0, StorageManager.Instance.storageSlots.Count - 1);
                storageFocusIndex = Mathf.Clamp(storageFocusIndex, 0, maxStorage);
                RefreshUI();
            }
        }
        if (statDisplay != null) statDisplay.RefreshStats();
    }

    private void ExecuteShopAction()
    {
        if (currentZone == UIZone.Inventory)
        {
            // 가방 -> 판매
            var invSlot = InventoryManager.Instance.slots[invFocusIndex];
            if (invSlot.item != null)
            {
                ShopManager.Instance.SellItem(invFocusIndex);
            }
        }
        else if (currentZone == UIZone.Shop)
        {
            // 상점 -> 구매
            if (ShopManager.Instance.shopItems.Count > shopFocusIndex)
            {
                ShopManager.Instance.BuyItem(shopFocusIndex);
            }
        }
        RefreshUI();
        if (statDisplay != null) statDisplay.RefreshStats();
    }

    // 기존 인벤토리/장비 로직 (변경 없음)
    private void ExecuteInventoryAction(bool isDoubleClick)
    {
        if (currentZone == UIZone.Equipment)
        {
            if (grabbedIndex == -1)
            {
                if (rightFocusIndex < 4) InventoryManager.Instance.UnequipItem(rightFocusIndex);
                else
                {
                    int quickIndex = rightFocusIndex - 4;
                    QuickSlotManager.Instance.UnequipQuickSlot(quickIndex);
                }
            }
            else
            {
                var grabbedSlot = InventoryManager.Instance.slots[grabbedIndex];
                if (rightFocusIndex < 4)
                {
                    if (grabbedSlot.item.itemType == ItemType.Equipment)
                    {
                        ItemData equip = grabbedSlot.item as ItemData;
                        if ((int)equip.equipType == rightFocusIndex)
                        {
                            InventoryManager.Instance.EquipItem(equip);
                            grabbedSlot.count--;
                            if (grabbedSlot.count <= 0) grabbedSlot.Clear();
                            DropGrabbedItem();
                        }
                    }
                }
                else
                {
                    if (grabbedSlot.item.itemType == ItemType.Consumable)
                    {
                        int quickIndex = rightFocusIndex - 4;
                        QuickSlotManager.Instance.EquipToQuickSlot(quickIndex, grabbedSlot);
                        DropGrabbedItem();
                    }
                }
            }
            RefreshUI();
            if (statDisplay != null) statDisplay.RefreshStats();
            return;
        }

        var currentSlotData = InventoryManager.Instance.slots[invFocusIndex];
        if (grabbedIndex == -1)
        {
            if (currentSlotData.item != null)
            {
                if (isDoubleClick)
                {
                    if (currentSlotData.item.itemType == ItemType.Equipment)
                    {
                        ItemData equip = currentSlotData.item as ItemData;
                        InventoryManager.Instance.EquipItem(equip);
                        currentSlotData.count--;
                        if (currentSlotData.count <= 0) currentSlotData.Clear();
                    }
                    else if (currentSlotData.item.itemType == ItemType.Consumable)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            if (QuickSlotManager.Instance.quickSlots[i].item == null)
                            {
                                QuickSlotManager.Instance.EquipToQuickSlot(i, currentSlotData);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    grabbedIndex = invFocusIndex;
                    floatingIcon.sprite = currentSlotData.item.icon;
                    floatingIcon.gameObject.SetActive(true);
                    uiSlots[grabbedIndex].iconImage.color = new Color(1, 1, 1, 0.5f);
                }
            }
        }
        else
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
        if (grabbedIndex != -1) { DropGrabbedItem(); return; }
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

        // ==========================================
        // [수정] 모드에 따라 갱신할 UI 분기
        // ==========================================
        if (isStorageMode)
        {
            if (StorageManager.Instance != null)
            {
                var sSlots = StorageManager.Instance.storageSlots;

                // 데이터 개수만큼 UI 슬롯이 부족하면 생성
                while (storageUiSlots.Count < sSlots.Count)
                {
                    GameObject go = Instantiate(slotPrefab, storageSlotParent);
                    storageUiSlots.Add(go.GetComponent<InventorySlotUI>());
                }

                // 슬롯 갱신 및 남는 UI 비활성화
                for (int i = 0; i < storageUiSlots.Count; i++)
                {
                    if (i < sSlots.Count)
                    {
                        storageUiSlots[i].gameObject.SetActive(true);
                        storageUiSlots[i].UpdateSlot(sSlots[i]);
                    }
                    else
                    {
                        storageUiSlots[i].gameObject.SetActive(false);
                    }
                }
            }
        }
        else if (isShopMode)
        {
            if (ShopManager.Instance != null)
            {
                var sItems = ShopManager.Instance.shopItems;

                while (shopUiSlots.Count < sItems.Count)
                {
                    GameObject go = Instantiate(slotPrefab, shopSlotParent);
                    shopUiSlots.Add(go.GetComponent<InventorySlotUI>());
                }

                for (int i = 0; i < shopUiSlots.Count; i++)
                {
                    if (i < sItems.Count)
                    {
                        shopUiSlots[i].gameObject.SetActive(true);
                        // 슬롯UI 재활용 (가짜 ItemSlot 만들어서 던져줌)
                        ItemSlot tempSlot = new ItemSlot();
                        tempSlot.item = sItems[i];
                        tempSlot.count = 1; // 상점엔 수량 무제한 느낌으로 1 고정
                        shopUiSlots[i].UpdateSlot(tempSlot);
                    }
                    else shopUiSlots[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            ItemData[] currentEquips = InventoryManager.Instance.equippedItems;
            for (int i = 0; i < 4; i++)
            {
                tempEquipData[i].item = currentEquips[i];
                tempEquipData[i].count = currentEquips[i] != null ? 1 : 0;
                if (equipSlots[i] != null) equipSlots[i].UpdateSlot(tempEquipData[i]);
            }

            if (QuickSlotManager.Instance != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (quickSlotUIs[i] != null) quickSlotUIs[i].UpdateSlot(QuickSlotManager.Instance.quickSlots[i]);
                }
            }
        }

        UpdateFocusVisuals();
    }

    private void UpdateFocusVisuals()
    {
        foreach (var slot in uiSlots) slot.SetFocus(false);
        foreach (var slot in equipSlots) slot.SetFocus(false);
        foreach (var slot in quickSlotUIs) slot.SetFocus(false);
        foreach (var slot in storageUiSlots) slot.SetFocus(false); // [추가]
        foreach (var slot in shopUiSlots) slot.SetFocus(false);

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
        else if (currentZone == UIZone.Storage)
        {
            // [추가] 창고 포커스 테두리 켜주기
            if (storageUiSlots.Count > storageFocusIndex && storageUiSlots[storageFocusIndex].gameObject.activeSelf)
            {
                storageUiSlots[storageFocusIndex].SetFocus(true);
                focusedItem = StorageManager.Instance.storageSlots[storageFocusIndex].item;
            }
        }
        else if (currentZone == UIZone.Shop)
        {
            if (shopUiSlots.Count > shopFocusIndex && shopUiSlots[shopFocusIndex].gameObject.activeSelf)
            {
                shopUiSlots[shopFocusIndex].SetFocus(true);
                focusedItem = ShopManager.Instance.shopItems[shopFocusIndex];
            }
        }

        if (focusedItem != null && tooltip != null) tooltip.Show(focusedItem);
        else if (tooltip != null) tooltip.Hide();
    }
}