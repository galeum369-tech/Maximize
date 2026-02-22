using UnityEngine;

[RequireComponent(typeof(ThemeSelector))] // ThemeSelector 스크립트가 같이 붙어있어야 작동함
public class ThemeSelectPortal : MonoBehaviour
{
    private PlayerInputHandler playerInput;
    private ThemeSelector themeSelector;

    private void Awake()
    {
        // 같은 오브젝트에 있는 ThemeSelector 컴포넌트를 가져옴
        themeSelector = GetComponent<ThemeSelector>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInput = collision.GetComponent<PlayerInputHandler>();
            if (playerInput != null)
            {
                // 상호작용 시 랜덤 입장 함수 연결
                playerInput.OnInteract += EnterRandomTheme;
                Debug.Log("랜덤 필드 진입 가능 (F키)");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (playerInput != null)
            {
                playerInput.OnInteract -= EnterRandomTheme;
                playerInput = null;
            }
        }
    }

    private void EnterRandomTheme()
    {
        // F키 연타로 인한 중복 실행 방지
        if (playerInput != null) playerInput.OnInteract -= EnterRandomTheme;

        /* --- [기존 UI 호출 로직 주석 처리] ---
        // UI 매니저를 통해 테마 선택 창을 띄움
        // UIManager.Instance.OpenThemeSelectionWindow();

        // UI가 열리면 플레이어 조작을 UI 모드로 전환
        if (playerInput != null)
        {
            // playerInput.OpenUI(true);
        }
        Debug.Log("테마 선택 UI를 엽니다.");
        ----------------------------------- */

        // ThemeSelector에 등록된 테마 중 하나를 랜덤으로 뽑아서 즉시 실행
        if (themeSelector != null && themeSelector.availableThemes.Length > 0)
        {
            int randomIndex = Random.Range(0, themeSelector.availableThemes.Length);
            Debug.Log($"<color=cyan>랜덤 테마 선택됨:</color> {themeSelector.availableThemes[randomIndex].name}");

            // ThemeSelector의 이동 로직 호출
            themeSelector.SelectTheme(randomIndex);
        }
        else
        {
            Debug.LogWarning("ThemeSelector에 등록된 테마 데이터가 없습니다!");
        }
    }
}