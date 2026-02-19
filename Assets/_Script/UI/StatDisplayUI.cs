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

    private void Update()
    {
        // 40칸 인벤토리나 상점을 열었을 때 실시간으로 갱신되게 Update에서 처리
        // (성능 최적화가 필요하면 이벤트 방식으로 바꿔도 됨)
        Refresh();
    }

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