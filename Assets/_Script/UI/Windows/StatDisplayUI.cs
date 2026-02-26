using UnityEngine;
using TMPro;

public class StatDisplayUI : MonoBehaviour
{
    [Header("UI 텍스트")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI droneAtkText; // [추가] 드론 공격력 텍스트
    public TextMeshProUGUI moneyText;

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (PlayerTransformManager.Instance == null) return;

        bool isMecha = PlayerTransformManager.Instance.IsMechaMode;

        // 1. 본체 스탯 표시
        if (isMecha)
        {
            var mState = PlayerTransformManager.Instance.mechaObject.GetComponent<MechaState>();
            if (mState != null)
            {
                hpText.text = $"HP: {mState.currentHp:F0} / {mState.FinalMaxHP:F0}";
                atkText.text = $"ATK: {mState.FinalAtk:F0}";
                defText.text = $"DEF: {mState.FinalDef:F0}";
                spdText.text = $"SPD: {mState.FinalSpd:F0}";
            }
        }
        else
        {
            var pState = PlayerTransformManager.Instance.humanObject.GetComponent<PlayerState>();
            if (pState != null)
            {
                hpText.text = $"HP: {pState.currentHp:F0} / {pState.FinalMaxHP:F0}";
                atkText.text = $"ATK: {pState.FinalAtk:F0}";
                defText.text = $"DEF: {pState.FinalDef:F0}";
                spdText.text = $"SPD: {pState.FinalSpd:F0}";
            }
        }

        // 2. [추가] 드론 스탯 표시
        if (droneAtkText != null && PlayerTransformManager.Instance.droneObject != null)
        {
            var dState = PlayerTransformManager.Instance.droneObject.GetComponent<DroneState>();
            if (dState != null)
            {
                droneAtkText.text = $"DRONE ATK: {dState.currentAtk:F0}";
            }
        }

        // 3. 재화 갱신
        if (moneyText != null && GameManager.Instance != null)
        {
            moneyText.text = $"Money: {GameManager.Instance.currentMoney}";
        }
    }
}