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

        if (PlayerTransformManager.Instance != null)
        {
            Vector3 targetPos = Vector3.zero;

            if (stateAfterLoad == GameState.Dungeon)
            {
                targetPos = Vector3.zero;
                Debug.Log("던전 진입: 스폰 위치 (0,0,0)");
            }
            else if (stateAfterLoad == GameState.Field)
            {
                if (GameManager.Instance.hasSavedPosition)
                {
                    targetPos = GameManager.Instance.savedFieldPosition;
                    GameManager.Instance.ClearSavedPosition();
                    Debug.Log("필드 복귀: 저장된 포탈 앞 스폰");
                }
                else
                {
                    targetPos = Vector3.zero;
                    Debug.Log("필드 진입: 기본 위치 스폰");
                }
            }
            else if (stateAfterLoad == GameState.Village)
            {
                targetPos = Vector3.zero;
                Debug.Log("마을 진입: 스폰 위치 (0,0,0)");

                // ==========================================
                // [핵심 추가] 마을로 들어왔을 때 플레이어 체력/에너지 초기화!
                // ==========================================
                PlayerTransformManager.Instance.ResetPlayerStatsForVillage();
            }

            PlayerTransformManager.Instance.WarpTo(targetPos);
        }
    }
}