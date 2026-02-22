using UnityEngine;

public static class UIUtils
{
    // 등급별 색상 반환
    public static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.12f, 0.8f, 0.12f), // 연두색
            ItemRarity.Rare => new Color(0.12f, 0.56f, 1f),     // 파란색
            ItemRarity.Epic => new Color(0.63f, 0.12f, 0.94f),   // 보라색
            _ => Color.white
        };
    }

    // 등급 이름을 한글로 반환 (옵션)
    public static string GetRarityName(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => "일반",
            ItemRarity.Uncommon => "우수",
            ItemRarity.Rare => "희귀",
            ItemRarity.Epic => "최상위",
            _ => ""
        };
    }
}