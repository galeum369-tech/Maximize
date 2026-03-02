using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("플레이어 상태 UI")]
    public Image hpFill;       // 체력바
    public TextMeshProUGUI hpText;

    [Header("재화 UI")]
    public TextMeshProUGUI goldText; // 소지 금액

    [Header("메카닉 변신 UI")]
    public Image mechaEnergyFill; // 변신 게이지바

    // ==========================================
    // [핵심 수정] 기존 Image 직접 참조를 버리고, 우리가 만든 SkillSlotUI 배열을 사용!
    // ==========================================
    [Header("스킬 슬롯 관리")]
    // 0: 일반스킬, 1: 특수스킬, 2: 폼체인지
    public SkillSlotUI[] skillSlots = new SkillSlotUI[3];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        // [핵심 추가] 게임 매니저의 골드 변경 이벤트를 구독!
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged += UpdateGold;

            // 씬 시작 시 현재 가지고 있는 돈으로 초기화
            UpdateGold(GameManager.Instance.currentMoney);
        }
    }

    private void OnDestroy()
    {
        // [핵심 추가] 파괴될 때 구독 해제
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnMoneyChanged -= UpdateGold;
        }
    }

    // 체력바 갱신
    public void UpdateHP(float currentHp, float maxHp)
    {
        if (hpFill != null) hpFill.fillAmount = currentHp / maxHp;
        if (hpText != null) hpText.text = $"{currentHp:F0} / {maxHp:F0}";
    }

    // 골드 갱신
    public void UpdateGold(int currentGold)
    {
        if (goldText != null) goldText.text = $" {currentGold:N0}";
    }

    // 메카 변신 게이지 갱신 (때릴 때마다 호출)
    public void UpdateMechaEnergy(float currentEnergy, float maxEnergy)
    {
        if (mechaEnergyFill != null)
        {
            mechaEnergyFill.fillAmount = currentEnergy / maxEnergy;
        }
    }

    // ==========================================
    // [추가된 스킬 관리 로직]
    // ==========================================

    // 폼 체인지 시 호출해서 슬롯 3개의 아이콘을 한 번에 스왑
    public void SwapSkillForm(bool isMecha)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            // SkillSlotUI에 있는 함수 호출
            skillSlots[i].ChangeForm(isMecha);
        }
    }

    // 매 프레임(혹은 쿨타임 돌 때) 플레이어 쪽에서 쿨타임 배열을 던져주면 UI에 갱신
    public void UpdateAllSkillCooldowns(float[] currentCooldowns, float[] maxCooldowns)
    {
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].UpdateCooldownUI(currentCooldowns[i], maxCooldowns[i]);
        }
    }
}