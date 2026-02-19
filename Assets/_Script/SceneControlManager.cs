using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneControlManager : MonoBehaviour
{
    public static SceneControlManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadTargetScene(string sceneName, GameState stateAfterLoad)
    {
        StartCoroutine(LoadSceneRoutine(sceneName, stateAfterLoad));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, GameState stateAfterLoad)
    {
        // 1. 페이드 아웃 등 전처리 (필요 시)
        yield return new WaitForSeconds(0.5f);

        // 2. 비동기 씬 로드 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return null;
        }

        // 3. [추가] 씬 로드 직후 위치 복구 로직 실행
        if (GameManager.Instance.hasSavedPosition)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = GameManager.Instance.savedFieldPosition;
                // 복구 후 플래그 초기화
                GameManager.Instance.ClearSavedPosition();
                Debug.Log("플레이어 위치 복구 완료");
            }
        }

        // 4. 게임 상태 변경
        GameManager.Instance.ChangeState(stateAfterLoad);

        // 5. 페이드 인 등 후처리 (필요 시)
    }
}