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
        GameManager.Instance.ChangeState(stateAfterLoad);
        yield return new WaitForSeconds(0.5f);

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        // --- [핵심 수정: WarpTo 함수 활용] ---
        if (PlayerTransformManager.Instance != null)
        {
            Vector3 targetPos = Vector3.zero; // 기본 스폰 지점

            if (stateAfterLoad == GameState.Dungeon)
            {
                // 던전 진입 시 무조건 (0, 0, 0)
                targetPos = Vector3.zero;
                Debug.Log("던전 진입: 스폰 위치 (0,0,0)");
            }
            else if (stateAfterLoad == GameState.Field)
            {
                if (GameManager.Instance.hasSavedPosition)
                {
                    // 던전 -> 필드 복귀: 저장해둔 포탈 좌표
                    targetPos = GameManager.Instance.savedFieldPosition;
                    GameManager.Instance.ClearSavedPosition();
                    Debug.Log("필드 복귀: 저장된 포탈 앞 스폰");
                }
                else
                {
                    // 마을 -> 필드 진입: 기본 (0, 0, 0)
                    targetPos = Vector3.zero;
                    Debug.Log("필드 진입: 기본 위치 스폰");
                }
            }
            else if (stateAfterLoad == GameState.Village)
            {
                targetPos = Vector3.zero;
            }

            // 부모와 자식 좌표를 한 번에 딱! 맞춰서 이동시킴
            PlayerTransformManager.Instance.WarpTo(targetPos);
        }
    }
}