using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // [필수] 씬 이동 감지
using System.Collections.Generic;

public enum UITab { Inventory, Settings }
public enum UIZone { Inventory, Equipment, Storage, Shop }

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("탭 & 구역 상태")]
    public GameObject inventoryPage;
    public GameObject settingsPage;
    public UITab currentTab = UITab.Inventory;
    public UIZone currentZone = UIZone.Inventory;

    private PlayerInputHandler currentInput;

    [Header("참조 (자동 연결됨)")]
    public GameObject inventoryRoot;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    [Header("판넬 스위칭")]
    public GameObject equipmentPanelRoot;
    public GameObject storagePanelRoot;
    public bool isStorageMode = false;

    public GameObject shopPanelRoot;
    public bool isShopMode = false;

    [Header("상점 UI")]
    public Transform shopSlotParent;
    private List<InventorySlotUI> shopUiSlots = new List<InventorySlotUI>();
    private int shopFocusIndex = 0;
    private int shopColumns = 5;

    [Header("장비/퀵슬롯 UI")]
    public InventorySlotUI[] equipSlots = new InventorySlotUI[4];
    public InventorySlotUI[] quickSlotUIs = new InventorySlotUI[3];
    private ItemSlot[] tempEquipData = new ItemSlot[4] { new ItemSlot(), new ItemSlot(), new ItemSlot(), new ItemSlot() };

    [Header("창고 UI")]
    public Transform storageSlotParent;
    private List<InventorySlotUI> storageUiSlots = new List<InventorySlotUI>();
    private int storageFocusIndex = 0;
    private int storageColumns = 5;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 초기화 시 비활성화
        if (inventoryRoot != null) inventoryRoot.SetActive(false);
        if (floatingIcon != null) floatingIcon.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트 구독 (이사 갈 때마다 짐 챙기기)
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ==========================================
    // [핵심 해결책] 씬이 로드되면 끊어진 UI 연결을 다시 복구한다!
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 연결이 끊겼는지(null이거나 Missing 상태인지) 확인하고 다시 찾음
        if (inventoryRoot == null) inventoryRoot = GameObject.Find("InventoryRoot");

        // 1. 패널 루트 찾기 (이름으로 찾음)
        if (shopPanelRoot == null) shopPanelRoot = FindObjectByName("ShopPanelRoot");
        if (storagePanelRoot == null) storagePanelRoot = FindObjectByName("StoragePanelRoot");
        if (equipmentPanelRoot == null) equipmentPanelRoot = FindObjectByName("EquipmentPanelRoot");

        // 2. 슬롯 부모(Grid Content) 찾기
        // 주의: 슬롯 부모들은 각 패널의 자식으로 있을 테니 경로를 잘 찾아야 함
        if (shopPanelRoot != null && shopSlotParent == null)
            shopSlotParent = shopPanelRoot.GetComponentInChildren<GridLayoutGroup>()?.transform;

        if (storagePanelRoot != null && storageSlotParent == null)
            storageSlotParent = storagePanelRoot.GetComponentInChildren<GridLayoutGroup>()?.transform;

        // 3. 메인 인벤토리 슬롯 부모 찾기
        if (inventoryRoot != null && slotParent == null)
        {
            // InventoryRoot 안에 있는 첫 번째 Grid Layout Group을 가방 슬롯으로 가정
            // (구조에 따라 다를 수 있으니 주의. 보통 InventoryPage -> ScrollView -> Viewport -> Content)
            Transform content = UIUtils.FindChildRecursive(inventoryRoot.transform, "Content");
            if (content != null) slotParent = content;
        }

        Debug.Log("[InventoryUI] UI 재연결 완료!");

        // 씬 넘어가면 UI는 기본적으로 닫힘 상태로 시작
        isOpen = false;
        if (inventoryRoot != null) inventoryRoot.SetActive(false);
    }

    // 이름으로 오브젝트 찾는 헬퍼 함수
    private GameObject FindObjectByName(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj == null)
        {
            // 비활성화된 오브젝트는 GameObject.Find로 못 찾으므로 전체 검색 (비용이 좀 들지만 씬 로드 시 1회니까 괜찮음)
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.hideFlags == HideFlags.None && go.name == name)
                    return go;
            }
        }
        return obj;
    }

    private void Start()
    {
        // 최초 슬롯 생성 (이미 있으면 스킵)
        if (uiSlots.Count == 0 && slotPrefab != null && slotParent != null)
        {
            for (int i = 0; i < InventoryManager.Instance.maxSlotCount; i++)
            {
                GameObject go = Instantiate(slotPrefab, slotParent);
                uiSlots.Add(go.GetComponent<InventorySlotUI>());
            }
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
            Transform targetTransform = transform; // 기본값

            if (currentZone == UIZone.Inventory && invFocusIndex < uiSlots.Count)
                targetTransform = uiSlots[invFocusIndex].transform;
            else if (currentZone == UIZone.Equipment)
            {
                if (rightFocusIndex < 4) targetTransform = equipSlots[rightFocusIndex].transform;
                else targetTransform = quickSlotUIs[rightFocusIndex - 4].transform;
            }
            else if (currentZone == UIZone.Storage)
            {
                if (storageUiSlots.Count > storageFocusIndex)
                    targetTransform = storageUiSlots[storageFocusIndex].transform;
                else if (storageSlotParent != null) targetTransform = storageSlotParent;
            }
            else if (currentZone == UIZone.Shop)
            {
                if (shopUiSlots.Count > shopFocusIndex)
                    targetTransform = shopUiSlots[shopFocusIndex].transform;
                else if (shopSlotParent != null) targetTransform = shopSlotParent;
            }

            floatingIcon.transform.position = targetTransform.position + offset;
        }
    }

    public void OpenStorageUI()
    {
        isStorageMode = true;
        currentZone = UIZone.Inventory;
        grabbedIndex = -1;

        if (equipmentPanelRoot != null) equipmentPanelRoot.SetActive(false);
        if (shopPanelRoot != null) shopPanelRoot.SetActive(false);
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
        if (inventoryRoot == null) return; // 안전장치

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

            isStorageMode = false;
            if (storagePanelRoot != null) storagePanelRoot.SetActive(false);

            isShopMode = false;
            if (shopPanelRoot != null) shopPanelRoot.SetActive(false);

            if (equipmentPanelRoot != null) equipmentPanelRoot.SetActive(true);
            currentZone = UIZone.Inventory;
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

        if (inventoryPage != null) inventoryPage.SetActive(currentTab == UITab.Inventory);
        if (settingsPage != null) settingsPage.SetActive(currentTab == UITab.Settings);

        currentZone = UIZone.Inventory;
        RefreshUI();
    }

    private void ToggleZone()
    {
        if (!isOpen || currentTab != UITab.Inventory) return;

        if (isStorageMode) currentZone = (currentZone == UIZone.Inventory) ? UIZone.Storage : UIZone.Inventory;
        else if (isShopMode) currentZone = (currentZone == UIZone.Inventory) ? UIZone.Shop : UIZone.Inventory;
        else currentZone = (currentZone == UIZone.Inventory) ? UIZone.Equipment : UIZone.Inventory;

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
                int maxShop = ShopManager.Instance.shopEntries.Count;
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

    private void ExecuteStorageAction()
    {
        if (currentZone == UIZone.Inventory)
        {
            var invSlot = InventoryManager.Instance.slots[invFocusIndex];
            if (invSlot.item != null)
            {
                StorageManager.Instance.DepositItem(invFocusIndex);
                RefreshUI();
            }
        }
        else if (currentZone == UIZone.Storage)
        {
            if (StorageManager.Instance.storageSlots.Count > storageFocusIndex)
            {
                StorageManager.Instance.WithdrawItem(storageFocusIndex);
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
            var invSlot = InventoryManager.Instance.slots[invFocusIndex];
            if (invSlot.item != null)
            {
                ShopManager.Instance.SellItem(invFocusIndex);
            }
        }
        else if (currentZone == UIZone.Shop)
        {
            if (ShopManager.Instance.shopEntries.Count > shopFocusIndex)
            {
                ShopManager.Instance.BuyItem(shopFocusIndex);
            }
        }
        RefreshUI();
        if (statDisplay != null) statDisplay.RefreshStats();
    }

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
                        if ((int)grabbedSlot.item.equipType == rightFocusIndex)
                        {
                            InventoryManager.Instance.EquipItem(grabbedSlot.item);
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
                        InventoryManager.Instance.EquipItem(currentSlotData.item);
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
        if (inventoryRoot == null) return; // 안전장치

        var dataSlots = InventoryManager.Instance.slots;
        // 슬롯 개수가 안 맞으면 다시 생성 (재연결 시 필요할 수 있음)
        if (uiSlots.Count == 0 && slotPrefab != null && slotParent != null)
        {
            // 기존 슬롯 다 지우고 다시
            foreach (Transform child in slotParent) Destroy(child.gameObject);
            uiSlots.Clear();
            for (int i = 0; i < InventoryManager.Instance.maxSlotCount; i++)
            {
                GameObject go = Instantiate(slotPrefab, slotParent);
                uiSlots.Add(go.GetComponent<InventorySlotUI>());
            }
        }

        for (int i = 0; i < uiSlots.Count; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].UpdateSlot(dataSlots[i]);
                if (i == grabbedIndex) uiSlots[i].iconImage.color = new Color(1, 1, 1, 0.5f);
                else uiSlots[i].iconImage.color = Color.white;
            }
        }

        // --- 상점/창고/장비 UI 갱신 ---
        // (UI 오브젝트가 존재할 때만 갱신하도록 null 체크 추가)
        if (isStorageMode && StorageManager.Instance != null && storageSlotParent != null)
        {
            var sSlots = StorageManager.Instance.storageSlots;
            // UI 슬롯 부족하면 채우기
            while (storageUiSlots.Count < sSlots.Count)
            {
                GameObject go = Instantiate(slotPrefab, storageSlotParent);
                storageUiSlots.Add(go.GetComponent<InventorySlotUI>());
            }
            // 갱신
            for (int i = 0; i < storageUiSlots.Count; i++)
            {
                if (storageUiSlots[i] == null) continue;
                if (i < sSlots.Count)
                {
                    storageUiSlots[i].gameObject.SetActive(true);
                    storageUiSlots[i].UpdateSlot(sSlots[i]);
                }
                else storageUiSlots[i].gameObject.SetActive(false);
            }
        }
        else if (isShopMode && ShopManager.Instance != null && shopSlotParent != null)
        {
            var sEntries = ShopManager.Instance.shopEntries;
            while (shopUiSlots.Count < sEntries.Count)
            {
                GameObject go = Instantiate(slotPrefab, shopSlotParent);
                shopUiSlots.Add(go.GetComponent<InventorySlotUI>());
            }
            for (int i = 0; i < shopUiSlots.Count; i++)
            {
                if (shopUiSlots[i] == null) continue;
                if (i < sEntries.Count)
                {
                    shopUiSlots[i].gameObject.SetActive(true);
                    ItemSlot tempSlot = new ItemSlot();
                    tempSlot.item = sEntries[i].resultItem;
                    tempSlot.count = 1;
                    shopUiSlots[i].UpdateSlot(tempSlot);
                }
                else shopUiSlots[i].gameObject.SetActive(false);
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
        // 모든 슬롯 포커스 끄기 (null 체크 포함)
        foreach (var slot in uiSlots) if (slot != null) slot.SetFocus(false);
        foreach (var slot in equipSlots) if (slot != null) slot.SetFocus(false);
        foreach (var slot in quickSlotUIs) if (slot != null) slot.SetFocus(false);
        foreach (var slot in storageUiSlots) if (slot != null) slot.SetFocus(false);
        foreach (var slot in shopUiSlots) if (slot != null) slot.SetFocus(false);

        ItemData focusedItem = null;

        if (currentZone == UIZone.Inventory && uiSlots.Count > invFocusIndex)
        {
            uiSlots[invFocusIndex].SetFocus(true);
            focusedItem = InventoryManager.Instance.slots[invFocusIndex].item;
        }
        else if (currentZone == UIZone.Equipment)
        {
            if (rightFocusIndex < 4 && equipSlots[rightFocusIndex] != null)
            {
                equipSlots[rightFocusIndex].SetFocus(true);
                focusedItem = tempEquipData[rightFocusIndex].item;
            }
            else if (rightFocusIndex >= 4)
            {
                int quickIndex = rightFocusIndex - 4;
                if (quickSlotUIs[quickIndex] != null)
                {
                    quickSlotUIs[quickIndex].SetFocus(true);
                    focusedItem = QuickSlotManager.Instance.quickSlots[quickIndex].item;
                }
            }
        }
        else if (currentZone == UIZone.Storage)
        {
            if (storageUiSlots.Count > storageFocusIndex && storageUiSlots[storageFocusIndex] != null && storageUiSlots[storageFocusIndex].gameObject.activeSelf)
            {
                storageUiSlots[storageFocusIndex].SetFocus(true);
                focusedItem = StorageManager.Instance.storageSlots[storageFocusIndex].item;
            }
        }
        else if (currentZone == UIZone.Shop)
        {
            if (shopUiSlots.Count > shopFocusIndex && shopUiSlots[shopFocusIndex] != null && shopUiSlots[shopFocusIndex].gameObject.activeSelf)
            {
                shopUiSlots[shopFocusIndex].SetFocus(true);
                var currentEntry = ShopManager.Instance.shopEntries[shopFocusIndex];
                if (tooltip != null) tooltip.Show(currentEntry.resultItem, currentEntry);
            }
        }

        // 상점 외에는 일반 툴팁 표시
        if (currentZone != UIZone.Shop)
        {
            if (focusedItem != null && tooltip != null) tooltip.Show(focusedItem);
            else if (tooltip != null) tooltip.Hide();
        }
    }
}