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
        if (startAsMecha) ToMecha(true);
        else ToHuman(true);
    }

    public void WarpTo(Vector3 targetPosition)
    {
        transform.position = targetPosition;

        if (humanObject != null) humanObject.transform.localPosition = Vector3.zero;
        if (mechaObject != null) mechaObject.transform.localPosition = Vector3.zero;
        if (droneObject != null) droneObject.transform.localPosition = Vector3.zero;

        if (humanObject != null) humanObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        if (mechaObject != null) mechaObject.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        Debug.Log($"[Warp] 플레이어 그룹 정렬 및 카메라 스냅 완료!");
    }

    public void ToggleMode()
    {
        if (!IsMechaMode) ToHuman();
        else ToMecha();
    }

    // --- 인간 -> 메카 변신 ---
    public void ToMecha(bool isInit = false)
    {
        if (!isInit)
        {
            mechaObject.transform.position = humanObject.transform.position;
            mechaObject.transform.rotation = humanObject.transform.rotation;
            if (droneObject != null) droneObject.transform.position = mechaObject.transform.position;
        }

        MechaState mState = mechaObject.GetComponent<MechaState>();
        if (mState != null)
        {
            if (isInit)
            {
                mState.RecalculateFinalStats();
                mState.currentHp = mState.FinalMaxHP;
            }
            UIManager.Instance?.UpdateHP(mState.currentHp, mState.FinalMaxHP);
        }

        humanObject.SetActive(false);
        droneObject.SetActive(false);
        mechaObject.SetActive(true);

        IsMechaMode = true;
        Debug.Log(">>> 메카닉 소환 완료!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(mechaObject.transform);
        }

        // [추가] 변신 즉시 카메라를 메카 위치로 순간이동!
        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        var mechaInput = mechaObject.GetComponent<PlayerInputHandler>();
        if (InventoryUI.Instance != null) InventoryUI.Instance.SetInputHandler(mechaInput);
        if (QuickSlotManager.Instance != null) QuickSlotManager.Instance.SetInputHandler(mechaInput);
    }

    // --- 메카 -> 인간 변신 ---
    public void ToHuman(bool isInit = false)
    {
        if (!isInit)
        {
            humanObject.transform.position = mechaObject.transform.position;
            humanObject.transform.rotation = mechaObject.transform.rotation;
            if (droneObject != null) droneObject.transform.position = humanObject.transform.position;
        }

        mechaObject.SetActive(false);
        humanObject.SetActive(true);
        if (droneObject != null) droneObject.SetActive(isDroneActiveInHuman);

        PlayerState pState = humanObject.GetComponent<PlayerState>();
        if (pState != null)
        {
            UIManager.Instance?.UpdateHP(pState.currentHp, pState.FinalMaxHP);
        }

        IsMechaMode = false;
        Debug.Log(">>> 파일럿 복귀 완료!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(humanObject.transform);
        }

        // [추가] 변신(또는 씬 시작) 즉시 카메라를 인간 위치로 순간이동!
        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        var humanInput = humanObject.GetComponent<PlayerInputHandler>();
        if (InventoryUI.Instance != null) InventoryUI.Instance.SetInputHandler(humanInput);
        if (QuickSlotManager.Instance != null) QuickSlotManager.Instance.SetInputHandler(humanInput);
    }
}