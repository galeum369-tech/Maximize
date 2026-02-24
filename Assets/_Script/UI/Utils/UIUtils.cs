using UnityEngine;

public static class UIUtils
{
    public static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.12f, 0.8f, 0.12f),
            ItemRarity.Rare => new Color(0.12f, 0.56f, 1f),
            ItemRarity.Epic => new Color(0.63f, 0.12f, 0.94f),
            _ => Color.white
        };
    }

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