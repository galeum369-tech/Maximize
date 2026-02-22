using UnityEngine;
using Unity.Cinemachine; // 유니티 6 (Cinemachine 3.x) 전용 네임스페이스

[RequireComponent(typeof(CinemachineCamera))]
public class CameraFollowManager : MonoBehaviour
{
    // 외부에서 쉽게 스냅 함수를 부를 수 있도록 싱글턴 적용
    public static CameraFollowManager Instance { get; private set; }

    private CinemachineCamera vcam;

    private void Awake()
    {
        // 싱글턴 초기화 및 씬 유지 세팅
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (Camera.main != null && Camera.main.transform.parent != transform)
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

    // [핵심 추가] 카메라를 타겟 위치로 즉시 순간이동 시키는 함수
    public void SnapToTarget()
    {
        if (vcam != null)
        {
            // 시네마신에게 "이전 프레임의 위치는 무효니까 부드럽게 오지 말고 즉시 이동해" 라고 지시
            vcam.PreviousStateIsValid = false;
        }
    }
}