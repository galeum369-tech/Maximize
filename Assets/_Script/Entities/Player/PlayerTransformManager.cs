using UnityEngine;

public class PlayerTransformManager : MonoBehaviour
{
    public static PlayerTransformManager Instance;

    [Header("플레이어 오브젝트 참조")]
    public GameObject humanObject;
    public GameObject mechaObject;
    public GameObject droneObject;

    [Header("설정")]
    public bool startAsMecha = false;
    public bool isDroneActiveInHuman = true;

    public bool IsMechaMode { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // [수정] 직접 input.OnMaximize를 구독하던 로직 삭제 (PlayerSkillController가 호출함)

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
            if (droneObject != null) droneObject.transform.position = mechaObject.transform.position;
        }

        // 2. 메카 체력/스탯 초기화 및 UI 갱신
        MechaState mState = mechaObject.GetComponent<MechaState>();
        if (mState != null)
        {
            if (isInit) // 씬 시작 등 초기화 때만 체력 꽉 채우기
            {
                mState.RecalculateFinalStats();
                mState.currentHp = mState.FinalMaxHP;
            }

            // [추가] 변신 즉시 UIManager에 메카 체력 정보 전달해서 화면 갱신!
            UIManager.Instance?.UpdateHP(mState.currentHp, mState.FinalMaxHP);
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
            if (droneObject != null) droneObject.transform.position = humanObject.transform.position;
        }

        // 2. 오브젝트 스위칭
        mechaObject.SetActive(false);
        humanObject.SetActive(true);
        if (droneObject != null) droneObject.SetActive(isDroneActiveInHuman);

        // [추가] 복귀 즉시 UIManager에 인간 폼 체력 정보 전달해서 화면 갱신!
        PlayerState pState = humanObject.GetComponent<PlayerState>();
        if (pState != null)
        {
            UIManager.Instance?.UpdateHP(pState.currentHp, pState.FinalMaxHP);
        }

        IsMechaMode = false;
        Debug.Log(">>> 파일럿 복귀 완료!");
    }
}