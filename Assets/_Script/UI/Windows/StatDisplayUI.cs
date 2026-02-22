using UnityEngine;
using TMPro;

public class StatDisplayUI : MonoBehaviour
{
    public PlayerData playerData; // SO 참조
    public PlayerState playerState; // 실제 인게임 상태 참조

    [Header("UI 텍스트")]
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public TextMeshProUGUI spdText;
    public TextMeshProUGUI moneyText;

    // StatDisplayUI.cs 수정 제안
    private void OnEnable()
    {
        if (playerState != null) playerState.OnStatsChanged += Refresh;
        Refresh(); // 켜질 때 한 번 갱신
    }
    private void OnDisable()
    {
        if (playerState != null) playerState.OnStatsChanged -= Refresh;
    }
    // Update() 함수는 삭제!

    public void Refresh()
    {
        // HP (현재/최대)
        hpText.text = $"HP: {Mathf.CeilToInt(playerState.currentHp)} / {playerState.FinalMaxHP} (Lv.{playerData.hpLevel})";

        // ATK
        atkText.text = $"ATK: {playerState.FinalAtk} (Lv.{playerData.attackLevel})";

        // DEF (방어력 % 표시)
        defText.text = $"DEF: {playerState.FinalDef}% (Lv.{playerData.defLevel})";

        // SPD
        spdText.text = $"SPD: {playerState.FinalSpd:F1} (Lv.{playerData.spdLevel})";

        // 돈
        moneyText.text = $"GOLD: {playerData.money:#,###}";
    }
}