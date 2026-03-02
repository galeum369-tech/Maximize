using UnityEngine;

public static class UIUtils
{
    // 1. 등급별 색상 반환
    public static Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.Common => Color.white,
            ItemRarity.Uncommon => new Color(0.12f, 0.8f, 0.12f), // 초록
            ItemRarity.Rare => new Color(0.12f, 0.56f, 1f),       // 파랑
            ItemRarity.Epic => new Color(0.63f, 0.12f, 0.94f),    // 보라
            _ => Color.white
        };
    }

    // 2. 등급별 한글 이름 반환
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

    // =========================================================
    // [추가] 인벤토리 UI 자동 연결에 필수적인 함수!
    // =========================================================
    public static Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;

            var result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }

    // =========================================================
    // [추가] 툴팁 텍스트 안에서 <color=#FF0000> 처럼 쓸 때 필요함
    // =========================================================
    public static string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }

    // =========================================================
    // [추가] 돈 표시할 때 1000 -> "1,000" 으로 예쁘게 바꿔줌
    // =========================================================
    public static string FormatNumber(int value)
    {
        return value.ToString("N0");
    }
}