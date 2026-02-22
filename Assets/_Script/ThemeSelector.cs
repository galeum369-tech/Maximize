using UnityEngine;

public class ThemeSelector : MonoBehaviour
{
    public ThemeStageData[] availableThemes;

    public void SelectTheme(int themeIndex)
    {
        if (themeIndex >= 0 && themeIndex < availableThemes.Length)
        {
            GameManager.Instance.SetTheme(availableThemes[themeIndex]);

            // [추가] 마을에서 나갈 때 무작위 번호(시드)를 하나 뽑아서 저장함
            int newSeed = Random.Range(1, 99999);
            PlayerPrefs.SetInt("CurrentFieldSeed", newSeed);

            SceneControlManager.Instance.LoadTargetScene("FieldScene", GameState.Field);
        }
    }
}