using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.SceneManagement; // [추가] 씬 이동 감지용

[RequireComponent(typeof(CinemachineCamera))]
public class CameraFollowManager : MonoBehaviour
{
    public static CameraFollowManager Instance { get; private set; }

    private CinemachineCamera vcam;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 메인 카메라도 같이 살려둠
            if (Camera.main != null)
            {
                DontDestroyOnLoad(Camera.main.gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        vcam = GetComponent<CinemachineCamera>();
    }

    // ==========================================
    // [핵심 추가] 씬이 로드될 때마다 중복된 카메라/오디오 리스너 제거
    // ==========================================
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬에 있는 모든 오디오 리스너를 찾음
        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);

        foreach (var listener in listeners)
        {
            // 내가 데리고 다니는 메인 카메라의 리스너가 아니라면?
            if (listener.gameObject != Camera.main.gameObject)
            {
                // 그 리스너가 붙은 오브젝트(보통 씬에 원래 있던 Main Camera)를 파괴해버림
                // (만약 카메라 기능은 살리고 싶다면 Destroy(listener)만 해도 됨)
                Debug.Log($"[CameraManager] 중복된 오디오 리스너 제거됨: {listener.gameObject.name}");
                Destroy(listener.gameObject);
            }
        }

        // 씬 이동 후 카메라 타겟 다시 잡기 (안전장치)
        SnapToTarget();
    }
    // ==========================================

    private void Update()
    {
        if (GameManager.Instance == null) return;

        Transform activePlayer = GameManager.Instance.GetActivePlayer();

        if (activePlayer != null && vcam.Target.TrackingTarget != activePlayer)
        {
            var targetConfig = vcam.Target;
            targetConfig.TrackingTarget = activePlayer;
            vcam.Target = targetConfig;

            Debug.Log($"[Camera] 카메라 타겟 변경됨: {activePlayer.name}");
        }
    }

    public void SnapToTarget()
    {
        if (GameManager.Instance == null) return;
        Transform target = GameManager.Instance.GetActivePlayer();

        if (target != null && vcam != null)
        {
            var targetConfig = vcam.Target;
            targetConfig.TrackingTarget = target;
            vcam.Target = targetConfig;

            // 시네머신 강제 이동 (OnTargetObjectWarped 사용)
            vcam.OnTargetObjectWarped(target, target.position - vcam.transform.position);
        }
    }
}