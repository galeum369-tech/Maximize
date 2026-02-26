using UnityEngine;
using UnityEngine.UI;
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

    [Header("참조")]
    public GameObject inventoryRoot;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    [Header("판넬 스위칭 (오른쪽 영역)")]
    public GameObject equipmentPanelRoot;
    public GameObject storagePanelRoot;
    public bool isStorageMode = false;

    [Header("판넬 스위칭 (상점 모드)")]
    public GameObject shopPanelRoot;
    public bool isShopMode = false;

    [Header("상점 UI")]
    public Transform shopSlotParent;
    private List<InventorySlotUI> shopUiSlots = new List<InventorySlotUI>();
    private int shopFocusIndex = 0;
    private int shopColumns = 5;

    [Header("장비/퀵슬롯 UI (오른쪽 판넬)")]
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
            else if (currentZone == UIZone.Storage)
            {
                if (storageUiSlots.Count > storageFocusIndex)
                    targetTransform = storageUiSlots[storageFocusIndex].transform;
                else targetTransform = storageSlotParent;
            }
            else
            {
                if (shopUiSlots.Count > shopFocusIndex)
                    targetTransform = shopUiSlots[shopFocusIndex].transform;
                else targetTransform = shopSlotParent;
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

        inventoryPage.SetActive(currentTab == UITab.Inventory);
        settingsPage.SetActive(currentTab == UITab.Settings);

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
        var dataSlots = InventoryManager.Instance.slots;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].UpdateSlot(dataSlots[i]);
            if (i == grabbedIndex) uiSlots[i].iconImage.color = new Color(1, 1, 1, 0.5f);
            else uiSlots[i].iconImage.color = Color.white;
        }

        if (isStorageMode)
        {
            if (StorageManager.Instance != null)
            {
                var sSlots = StorageManager.Instance.storageSlots;

                while (storageUiSlots.Count < sSlots.Count)
                {
                    GameObject go = Instantiate(slotPrefab, storageSlotParent);
                    storageUiSlots.Add(go.GetComponent<InventorySlotUI>());
                }

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
                var sEntries = ShopManager.Instance.shopEntries;

                while (shopUiSlots.Count < sEntries.Count)
                {
                    GameObject go = Instantiate(slotPrefab, shopSlotParent);
                    shopUiSlots.Add(go.GetComponent<InventorySlotUI>());
                }

                for (int i = 0; i < shopUiSlots.Count; i++)
                {
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
        foreach (var slot in storageUiSlots) slot.SetFocus(false);
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
            if (storageUiSlots.Count > storageFocusIndex && storageUiSlots[storageFocusIndex].gameObject.activeSelf)
            {
                storageUiSlots[storageFocusIndex].SetFocus(true);
                focusedItem = StorageManager.Instance.storageSlots[storageFocusIndex].item;
            }
        }

        if (currentZone == UIZone.Shop && shopUiSlots.Count > shopFocusIndex && shopUiSlots[shopFocusIndex].gameObject.activeSelf)
        {
            shopUiSlots[shopFocusIndex].SetFocus(true);
            var currentEntry = ShopManager.Instance.shopEntries[shopFocusIndex];
            if (tooltip != null) tooltip.Show(currentEntry.resultItem, currentEntry);
        }
        else
        {
            if (focusedItem != null && tooltip != null) tooltip.Show(focusedItem);
            else if (tooltip != null) tooltip.Hide();
        }
    }
}