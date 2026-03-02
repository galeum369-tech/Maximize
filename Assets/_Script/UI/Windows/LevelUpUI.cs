using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement; // [필수] 씬 이동 감지

public class LevelUpUI : MonoBehaviour
{
    public static LevelUpUI Instance { get; private set; }

    [Header("참조 (자동 연결됨)")]
    public GameObject uiRoot;

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 넘어가도 매니저는 생존
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (uiRoot != null) uiRoot.SetActive(false);
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ==========================================
    // [핵심] 씬이 로드되면 끊어진 UI 연결을 이름으로 찾아서 복구!
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 1. UI Root 찾기 (이름이 정확해야 함!)
        if (uiRoot == null) uiRoot = GameObject.Find("LevelUpPanelRoot");

        if (uiRoot == null) return; // UI가 없는 씬일 수도 있으니 안전장치

        // 2. 자식 컴포넌트들 다시 찾아서 연결 (UIUtils 활용)
        // (텍스트)
        humanLvText = FindText("HumanLvText");
        humanCostText = FindText("HumanCostText");

        mechaLvText = FindText("MechaLvText");
        mechaCostText = FindText("MechaCostText");

        droneLvText = FindText("DroneLvText");
        droneCostText = FindText("DroneCostText");

        currentMoneyText = FindText("CurrentMoneyText");

        // (포커스 아웃라인)
        focusOutlines[0] = FindChildObject("FocusOutline_Human");
        focusOutlines[1] = FindChildObject("FocusOutline_Mecha");
        focusOutlines[2] = FindChildObject("FocusOutline_Drone");

        // 3. 초기화 (꺼두기)
        uiRoot.SetActive(false);
        Debug.Log("[LevelUpUI] UI 재연결 완료!");
    }

    // 텍스트 컴포넌트 찾는 헬퍼 함수
    private TextMeshProUGUI FindText(string name)
    {
        if (uiRoot == null) return null;
        Transform t = UIUtils.FindChildRecursive(uiRoot.transform, name);
        return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
    }

    // 일반 오브젝트 찾는 헬퍼 함수
    private GameObject FindChildObject(string name)
    {
        if (uiRoot == null) return null;
        Transform t = UIUtils.FindChildRecursive(uiRoot.transform, name);
        return t != null ? t.gameObject : null;
    }

    public void OpenUI()
    {
        // [핵심 1] PlayerData 자동 획득
        if (playerData == null && PlayerTransformManager.Instance != null)
        {
            if (PlayerTransformManager.Instance.humanObject != null)
            {
                PlayerState pState = PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>();
                if (pState != null) playerData = pState.baseData;
            }
        }

        if (uiRoot != null) uiRoot.SetActive(true);
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
        if (uiRoot != null) uiRoot.SetActive(false);

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
        if (playerData == null) return;

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
        if (humanLvText == null) return; // UI 연결 안됐으면 패스

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
        RefreshUI();

        // [핵심 2] 스탯 강제 갱신!
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ForceStatUpdate();
        }
    }
}