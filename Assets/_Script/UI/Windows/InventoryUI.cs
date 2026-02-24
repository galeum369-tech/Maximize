using UnityEngine;
using System.Collections.Generic;

// 탭 종류 정의
public enum UITab { Inventory, Settings }

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("탭 페이지 참조")]
    public GameObject inventoryPage; // 인벤토리 + 장비창이 포함된 판넬
    public GameObject settingsPage;  // 설정(게임 종료 등)이 포함된 판넬
    public UITab currentTab = UITab.Inventory;

    [Header("참조")]
    public PlayerInputHandler inputHandler;
    public GameObject inventoryRoot;
    public Transform slotParent;
    public GameObject slotPrefab;

    [Header("UI 컴포넌트")]
    public ItemTooltipUI tooltip;
    public StatDisplayUI statDisplay;

    private List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    // 인벤토리 관련 변수
    private int currentFocusIndex = 0;
    private int columns = 8;

    // 설정창 관련 변수 (단순 수직 리스트 예시)
    private int settingsFocusIndex = 0;
    private int settingsCount = 3; // 마을 귀환, 타이틀로, 게임 종료 등

    private bool isOpen = false;

    private void Awake()
    {
        Instance = this;
        inventoryRoot.SetActive(false);
    }

    private void Start()
    {
        if (inputHandler != null)
        {
            // Tab 키: 열고 닫기
            inputHandler.OnInventory += ToggleInventory;
            inputHandler.OnCloseUI += ToggleInventory;

            // 방향키 및 버튼
            inputHandler.OnNavigate += HandleNavigation;
            inputHandler.OnSubmit += HandleSubmit; // Z 키
            inputHandler.OnCancel += HandleCancel; // C 키 (닫기)

            // Q, E 키: 탭 전환
            inputHandler.OnPrevTab += () => ChangeTab(-1);
            inputHandler.OnNextTab += () => ChangeTab(1);
        }

        InitSlots();
    }

    public void ToggleInventory()
    {
        isOpen = !isOpen;
        inventoryRoot.SetActive(isOpen);

        if (isOpen)
        {
            inputHandler.OpenUI(true);
            SetTab(UITab.Inventory); // 열 때는 항상 인벤토리 탭부터 시작
        }
        else
        {
            inputHandler.OpenUI(false);
            tooltip.Hide();
        }
    }

    // --- [탭 관리 로직] ---

    private void ChangeTab(int direction)
    {
        if (!isOpen) return;

        // Enum 순환 계산
        int totalTabs = System.Enum.GetNames(typeof(UITab)).Length;
        int nextTab = ((int)currentTab + direction + totalTabs) % totalTabs;

        SetTab((UITab)nextTab);
    }

    private void SetTab(UITab newTab)
    {
        currentTab = newTab;

        // 판넬 활성/비활성
        inventoryPage.SetActive(currentTab == UITab.Inventory);
        settingsPage.SetActive(currentTab == UITab.Settings);

        // 탭 전환 시 초기화
        if (currentTab == UITab.Inventory)
        {
            RefreshUI();
        }
        else
        {
            tooltip.Hide(); // 설정창에서는 툴팁 숨김
            settingsFocusIndex = 0;
            UpdateSettingsVisuals();
        }
    }

    // --- [조작 핸들러] ---

    private void HandleNavigation(Vector2 dir)
    {
        if (!isOpen) return;

        if (currentTab == UITab.Inventory)
        {
            NavigateInventory(dir);
        }
        else
        {
            NavigateSettings(dir);
        }
    }

    private void HandleSubmit()
    {
        if (!isOpen) return;

        if (currentTab == UITab.Inventory)
        {
            ExecuteInventoryAction();
        }
        else
        {
            ExecuteSettingsAction();
        }
    }

    private void HandleCancel()
    {
        if (isOpen) ToggleInventory(); // C 키 누르면 UI 닫기
    }

    // --- [세부 로직: 인벤토리] ---

    private void NavigateInventory(Vector2 dir)
    {
        int row = currentFocusIndex / columns;
        int col = currentFocusIndex % columns;

        if (dir.x > 0.5f) col++;
        else if (dir.x < -0.5f) col--;
        if (dir.y > 0.5f) row--;
        else if (dir.y < -0.5f) row++;

        col = Mathf.Clamp(col, 0, columns - 1);
        row = Mathf.Clamp(row, 0, 4);

        currentFocusIndex = row * columns + col;
        UpdateFocusVisuals();
    }

    private void ExecuteInventoryAction()
    {
        var slot = InventoryManager.Instance.slots[currentFocusIndex];
        if (slot.item != null)
        {
            if (slot.item is EquipmentData equip)
            {
                Debug.Log($"{equip.itemName} 장착 완료 (Z키)");
                // 실제 장착 함수 연결: PlayerState.Instance.Equip(equip);
            }
            RefreshUI();
        }
    }

    // --- [세부 로직: 설정창] ---

    private void NavigateSettings(Vector2 dir)
    {
        // 설정창은 위아래 리스트 형태라고 가정
        if (dir.y > 0.5f) settingsFocusIndex--;
        else if (dir.y < -0.5f) settingsFocusIndex++;

        settingsFocusIndex = Mathf.Clamp(settingsFocusIndex, 0, settingsCount - 1);
        UpdateSettingsVisuals();
    }

    private void ExecuteSettingsAction()
    {
        // Z키 눌렀을 때의 동작
        switch (settingsFocusIndex)
        {
            case 0: Debug.Log("마을로 돌아가기"); break;
            case 1: Debug.Log("타이틀로 이동"); break;
            case 2: Debug.Log("게임 종료"); Application.Quit(); break;
        }
    }

    // --- [공통 업데이트] ---

    public void RefreshUI()
    {
        var dataSlots = InventoryManager.Instance.slots;
        for (int i = 0; i < uiSlots.Count; i++)
        {
            uiSlots[i].UpdateSlot(dataSlots[i]);
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
        if (item != null) tooltip.Show(item);
        else tooltip.Hide();
    }

    private void UpdateSettingsVisuals()
    {
        // 설정창의 버튼들 하이라이트 로직 (추후 구현)
        Debug.Log($"현재 설정 포커스: {settingsFocusIndex}");
    }

    private void InitSlots()
    {
        foreach (Transform child in slotParent) Destroy(child.gameObject);
        uiSlots.Clear();
        for (int i = 0; i < 40; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            uiSlots.Add(go.GetComponent<InventorySlotUI>());
        }
    }
}