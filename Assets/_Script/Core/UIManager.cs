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

    // [추가] 메카 변신 게이지 UI
    [Header("메카닉 변신 UI")]
    public Image mechaEnergyFill; // 변신 게이지바 (0부터 꽉 찰 때까지)

    // [추가] 스킬 쿨타임 UI
    [Header("스킬 쿨타임 UI")]
    public Image mechaSkillCooldownOverlay; // 메카 스킬 아이콘 위를 덮을 반투명 검은색 이미지
    public Image droneSkillCooldownOverlay; // 드론 스킬 가림막 이미지

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
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
        if (goldText != null) goldText.text = $"💰 {currentGold:N0}";
    }

    // --- [추가] 메카 변신 게이지 갱신 ---
    // 플레이어가 적을 때릴 때마다 이걸 호출해주면 됨
    public void UpdateMechaEnergy(float currentEnergy, float maxEnergy)
    {
        if (mechaEnergyFill != null)
        {
            mechaEnergyFill.fillAmount = currentEnergy / maxEnergy;
        }
    }

    // --- [추가] 쿨타임 UI 업데이트 ---
    // ratio: 남은 시간 비율 (1이면 쿨타임 꽉 찬 상태, 0이면 쿨타임 끝나서 스킬 사용 가능)
    public void UpdateMechaSkillCooldown(float ratio)
    {
        if (mechaSkillCooldownOverlay != null)
            mechaSkillCooldownOverlay.fillAmount = ratio;
    }

    public void UpdateDroneSkillCooldown(float ratio)
    {
        if (droneSkillCooldownOverlay != null)
            droneSkillCooldownOverlay.fillAmount = ratio;
    }
}