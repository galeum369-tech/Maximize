using UnityEngine;
using TMPro;

public class StatDisplayUI : MonoBehaviour
{
    [Header("UI 텍스트")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI moneyText;

    private void OnEnable()
    {
        RefreshStats();
    }

    public void RefreshStats()
    {
        if (PlayerTransformManager.Instance == null) return;

        bool isMecha = PlayerTransformManager.Instance.IsMechaMode;

        // 현재 모드에 따라 State 스크립트를 가져와서 수치 표시
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

        if (GameManager.Instance != null)
        {
            moneyText.text = $"💰 {GameManager.Instance.currentMoney:N0}";
        }
    }
}