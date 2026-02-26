using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance { get; private set; }

    [Header("참조")]
    public GameObject uiRoot;

    // [수정] 이제 인스펙터에서 안 넣어도 됨! (HideInInspector 처리)
    [HideInInspector] public PlayerData playerData;

    [Header("UI 포커스 연출")]
    public GameObject[] focusOutlines = new GameObject[3];
    private int currentFocusIndex = 0;

    [Header("Human 업그레이드")]
    public TextMeshProUGUI humanLvText;
    public TextMeshProUGUI humanCostText;

    [Header("Mecha 업그레이드")]
    public TextMeshProUGUI mechaLvText;
    public TextMeshProUGUI mechaCostText;

    [Header("Drone 업그레이드")]
    public TextMeshProUGUI droneLvText;
    public TextMeshProUGUI droneCostText;

    [Header("소지 재화")]
    public TextMeshProUGUI currentMoneyText;

    private PlayerInputHandler currentInput;

    private void Awake()
    {
        Instance = this;
        if (uiRoot != null) uiRoot.SetActive(false);
    }

    public void OpenUI()
    {
        // ==========================================
        // [핵심 1] UI를 열 때 활성화된 플레이어의 상태창에서 PlayerData를 자동으로 훔쳐옴!
        // ==========================================
        if (playerData == null && PlayerTransformManager.Instance != null)
        {
            // 인간 폼에 무조건 baseData가 있으니 거기서 빼옴
            PlayerState pState = PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>();
            if (pState != null) playerData = pState.baseData;
        }

        uiRoot.SetActive(true);
        currentFocusIndex = 0;
        RefreshUI();

        if (GameManager.Instance != null && GameManager.Instance.GetActivePlayer() != null)
        {
            currentInput = GameManager.Instance.GetActivePlayer().GetComponent<PlayerInputHandler>();
            if (currentInput != null)
            {
                currentInput.OpenUI(true);
                currentInput.OnCloseUI += CloseUI;
                currentInput.OnCancel += CloseUI;
                currentInput.OnNavigate += HandleNavigate;
                currentInput.OnSubmit += HandleSubmit;
            }
        }
    }

    public void CloseUI()
    {
        uiRoot.SetActive(false);
        if (currentInput != null)
        {
            currentInput.OpenUI(false);
            currentInput.OnCloseUI -= CloseUI;
            currentInput.OnCancel -= CloseUI;
            currentInput.OnNavigate -= HandleNavigate;
            currentInput.OnSubmit -= HandleSubmit;
            currentInput = null;
        }
    }

    private void HandleNavigate(Vector2 dir)
    {
        if (dir.y < 0) currentFocusIndex++;
        else if (dir.y > 0) currentFocusIndex--;
        currentFocusIndex = Mathf.Clamp(currentFocusIndex, 0, 2);
        UpdateFocusVisuals();
    }

    private void HandleSubmit()
    {
        switch (currentFocusIndex)
        {
            case 0:
                if (GameManager.Instance.currentMoney >= playerData.GetHumanUpgradeCost()) UpgradeHuman();
                else Debug.Log("골드가 부족해!");
                break;
            case 1:
                if (GameManager.Instance.currentMoney >= playerData.GetMechaUpgradeCost()) UpgradeMecha();
                else Debug.Log("골드가 부족해!");
                break;
            case 2:
                if (GameManager.Instance.currentMoney >= playerData.GetDroneUpgradeCost()) UpgradeDrone();
                else Debug.Log("골드가 부족해!");
                break;
        }
    }

    private void RefreshUI()
    {
        if (playerData == null || GameManager.Instance == null) return;

        currentMoneyText.text = $"보유 골드: {GameManager.Instance.currentMoney:N0} G";

        humanLvText.text = $"Human Lv.{playerData.humanLevel}";
        humanCostText.text = $"{playerData.GetHumanUpgradeCost()} G";
        humanCostText.color = GameManager.Instance.currentMoney >= playerData.GetHumanUpgradeCost() ? Color.white : Color.red;

        mechaLvText.text = $"Mecha Lv.{playerData.mechaLevel}";
        mechaCostText.text = $"{playerData.GetMechaUpgradeCost()} G";
        mechaCostText.color = GameManager.Instance.currentMoney >= playerData.GetMechaUpgradeCost() ? Color.white : Color.red;

        droneLvText.text = $"Drone Lv.{playerData.droneLevel}";
        droneCostText.text = $"{playerData.GetDroneUpgradeCost()} G";
        droneCostText.color = GameManager.Instance.currentMoney >= playerData.GetDroneUpgradeCost() ? Color.white : Color.red;

        UpdateFocusVisuals();
    }

    private void UpdateFocusVisuals()
    {
        for (int i = 0; i < focusOutlines.Length; i++)
        {
            if (focusOutlines[i] != null) focusOutlines[i].SetActive(i == currentFocusIndex);
        }
    }

    private void UpgradeHuman() { GameManager.Instance.UseMoney(playerData.GetHumanUpgradeCost()); playerData.humanLevel++; ProcessUpgradeSuccess(); }
    private void UpgradeMecha() { GameManager.Instance.UseMoney(playerData.GetMechaUpgradeCost()); playerData.mechaLevel++; ProcessUpgradeSuccess(); }
    private void UpgradeDrone() { GameManager.Instance.UseMoney(playerData.GetDroneUpgradeCost()); playerData.droneLevel++; ProcessUpgradeSuccess(); }

    private void ProcessUpgradeSuccess()
    {
        Debug.Log("레벨업 성공!");
        RefreshUI(); // 현재 레벨업 창 텍스트 즉시 갱신

        // ==========================================
        // [핵심 2] 스탯 강제 갱신! 
        // 이걸 부르면 현재 켜져있는 폼(인간 or 메카)이 스탯을 다시 계산하고, HUD 체력바도 새로 고침!
        // ==========================================
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ForceStatUpdate();
        }

        // 인벤토리가 열려있는 상태는 아니겠지만, 나중에 인벤토리를 열면
        // StatDisplayUI의 OnEnable()이 작동해서 자동으로 최신 스탯이 반영됨.
    }
}