using UnityEngine;

public class ThemeSelector : MonoBehaviour
{
    // 테마별 씬 이름 혹은 ID
    public void SelectTheme(int themeIndex)
    {
        string targetScene = "";
        switch (themeIndex)
        {
            case 0: targetScene = "Field_Forest"; break;
            case 1: targetScene = "Field_Desert"; break;
        }

        if (targetScene != "")
        {
            // SceneControlManager를 통해 이동
            SceneControlManager.Instance.LoadTargetScene(targetScene, GameState.Dungeon);
        }
    }
}