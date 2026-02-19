using UnityEngine;

public class ThemeSelectPortal : MonoBehaviour
{
    private PlayerInputHandler playerInput;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                // 상호작용 시 UI 오픈 함수 연결
                playerInput.OnInteract += OpenThemeUI;
                Debug.Log("테마 선택 가능");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerInput != null)
            {
                playerInput.OnInteract -= OpenThemeUI;
                playerInput = null;
            }
        }
    }

    private void OpenThemeUI()
    {
        // UI 매니저를 통해 테마 선택 창을 띄움
        // UIManager.Instance.OpenThemeSelectionWindow();

        // UI가 열리면 플레이어 조작을 UI 모드로 전환
        if (playerInput != null)
        {
            playerInput.OpenUI(true);
        }

        Debug.Log("테마 선택 UI를 엽니다.");
    }
}