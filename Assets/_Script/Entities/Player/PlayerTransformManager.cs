using UnityEngine;

public class PlayerTransformManager : MonoBehaviour
{
    public static PlayerTransformManager Instance;

    [Header("플레이어 오브젝트 참조")]
    public GameObject humanObject; // 파일럿
    public GameObject mechaObject; // 메카닉
    public GameObject droneObject; // 드론 (추가됨)

    [Header("설정")]
    public bool startAsMecha = false; // 테스트용 시작 모드
    public bool isDroneActiveInHuman = true; // 인간일 때 드론 사용 여부

    // 현재 상태 프로퍼티
    public bool IsMechaMode { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. 각 캐릭터의 입력 핸들러를 찾아 이벤트 구독
        var humanInput = humanObject.GetComponent<PlayerInputHandler>();
        var mechaInput = mechaObject.GetComponent<PlayerInputHandler>();

        if (humanInput != null) humanInput.OnMaximize += ToggleMode;
        if (mechaInput != null) mechaInput.OnMaximize += ToggleMode;

        // 2. 초기 모드 설정
        if (startAsMecha) ToMecha(true);
        else ToHuman(true);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (humanObject != null)
        {
            var humanInput = humanObject.GetComponent<PlayerInputHandler>();
            if (humanInput != null) humanInput.OnMaximize -= ToggleMode;
        }

        if (mechaObject != null)
        {
            var mechaInput = mechaObject.GetComponent<PlayerInputHandler>();
            if (mechaInput != null) mechaInput.OnMaximize -= ToggleMode;
        }
    }
    
    public void WarpTo(Vector3 targetPosition)
    {
        // 1. 부모(빈 오브젝트 컨테이너)를 목적지로 이동
        transform.position = targetPosition;

        // 2. 자식들 로컬 좌표 0으로 초기화
        if (humanObject != null) humanObject.transform.localPosition = Vector3.zero;
        if (mechaObject != null) mechaObject.transform.localPosition = Vector3.zero;
        if (droneObject != null) droneObject.transform.localPosition = Vector3.zero;

        // 물리 관성 초기화
        if (humanObject != null) humanObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        if (mechaObject != null) mechaObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        // [핵심 추가] 캐릭터가 워프했으니, 카메라도 천천히 따라오지 말고 즉시 텔레포트하도록 명령!
        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        Debug.Log($"[Warp] 플레이어 그룹 정렬 및 카메라 스냅 완료!");
    }

    public void ToggleMode()
    {
        if (IsMechaMode) ToHuman();
        else ToMecha();
    }

    // --- 인간 -> 메카 변신 ---
    public void ToMecha(bool isInit = false)
    {
        // 1. 위치 동기화 (인간 -> 메카)
        if (!isInit)
        {
            mechaObject.transform.position = humanObject.transform.position;
            mechaObject.transform.rotation = humanObject.transform.rotation;

            // 드론 위치도 메카 쪽으로 즉시 이동 (부드러운 추적 중이라면 생략 가능)
            if (droneObject != null)
            {
                droneObject.transform.position = mechaObject.transform.position;
            }
        }

        // 2. 메카 체력/스탯 초기화
        MechaState mState = mechaObject.GetComponent<MechaState>();
        if (mState != null)
        {
            mState.RecalculateFinalStats();
            mState.currentHp = mState.FinalMaxHP;
        }

        // 3. 오브젝트 스위칭
        humanObject.SetActive(false);
        droneObject.SetActive(false);
        mechaObject.SetActive(true);


        IsMechaMode = true;
        Debug.Log(">>> 메카닉 소환 완료!");
    }

    // --- 메카 -> 인간 변신 ---
    public void ToHuman(bool isInit = false)
    {
        // 1. 위치 동기화 (메카 -> 인간)
        if (!isInit)
        {
            humanObject.transform.position = mechaObject.transform.position;
            humanObject.transform.rotation = mechaObject.transform.rotation;

            // 드론 위치 동기화
            if (droneObject != null)
            {
                droneObject.transform.position = humanObject.transform.position;
            }
        }

        // 2. 오브젝트 스위칭
        mechaObject.SetActive(false);
        humanObject.SetActive(true);
        droneObject.SetActive(true);

        // 인간일 때 드론 사용 여부에 따라 활성화/비활성화
        if (droneObject != null)
        {
            droneObject.SetActive(isDroneActiveInHuman);
        }

        IsMechaMode = false;
        Debug.Log(">>> 파일럿 복귀 완료!");
    }
}