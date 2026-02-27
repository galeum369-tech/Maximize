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
    }

    public void ToMecha(bool isInit = false)
    {
        IsMechaMode = true;

        if (!isInit)
        {
            mechaObject.transform.position = humanObject.transform.position;
            mechaObject.transform.rotation = humanObject.transform.rotation;
            if (droneObject != null) droneObject.transform.position = mechaObject.transform.position;
        }

        humanObject.SetActive(false);
        mechaObject.SetActive(true);
        if (droneObject != null) droneObject.SetActive(true);

        Debug.Log(">>> 메카 탑승 완료!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(mechaObject.transform);
        }

        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        var mechaInput = mechaObject.GetComponent<PlayerInputHandler>();
        if (InventoryUI.Instance != null) InventoryUI.Instance.SetInputHandler(mechaInput);
        if (QuickSlotManager.Instance != null) QuickSlotManager.Instance.SetInputHandler(mechaInput);

        if (InventoryManager.Instance != null) InventoryManager.Instance.ForceStatUpdate();
        RefreshOpenUIs();
    }

    public void ToHuman(bool isInit = false)
    {
        IsMechaMode = false;

        if (!isInit)
        {
            humanObject.transform.position = mechaObject.transform.position;
            humanObject.transform.rotation = mechaObject.transform.rotation;
            if (droneObject != null) droneObject.transform.position = humanObject.transform.position;
        }

        mechaObject.SetActive(false);
        humanObject.SetActive(true);
        if (droneObject != null) droneObject.SetActive(isDroneActiveInHuman);

        Debug.Log(">>> 파일럿 복귀 완료!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(humanObject.transform);
        }

        if (CameraFollowManager.Instance != null)
        {
            CameraFollowManager.Instance.SnapToTarget();
        }

        var humanInput = humanObject.GetComponent<PlayerInputHandler>();
        if (InventoryUI.Instance != null) InventoryUI.Instance.SetInputHandler(humanInput);
        if (QuickSlotManager.Instance != null) QuickSlotManager.Instance.SetInputHandler(humanInput);

        if (InventoryManager.Instance != null) InventoryManager.Instance.ForceStatUpdate();
        RefreshOpenUIs();
    }

    public void RefreshOpenUIs()
    {
        if (InventoryUI.Instance != null && InventoryUI.Instance.isOpen)
        {
            InventoryUI.Instance.RefreshUI();
            if (InventoryUI.Instance.statDisplay != null)
                InventoryUI.Instance.statDisplay.RefreshStats();
        }
    }

    // ==========================================
    // [핵심 추가] 마을 귀환 시 체력 회복 & 에너지 초기화
    // ==========================================
    public void ResetPlayerStatsForVillage()
    {
        // 1. 인간 폼 풀피 회복
        if (humanObject != null)
        {
            PlayerState pState = humanObject.GetComponent<PlayerState>();
            if (pState != null)
            {
                pState.currentHp = pState.FinalMaxHP;
                if (!IsMechaMode) UIManager.Instance?.UpdateHP(pState.currentHp, pState.FinalMaxHP);
            }
        }

        // 2. 메카 폼 풀피 회복
        if (mechaObject != null)
        {
            MechaState mState = mechaObject.GetComponent<MechaState>();
            if (mState != null)
            {
                mState.currentHp = mState.FinalMaxHP;
                if (IsMechaMode) UIManager.Instance?.UpdateHP(mState.currentHp, mState.FinalMaxHP);
            }
        }

        // 3. 메카 변신 에너지 게이지 강제 초기화
        if (PlayerSkillController.Instance != null)
        {
            PlayerSkillController.Instance.ResetMechaEnergy();
        }

        Debug.Log("마을 도착: 플레이어 체력 회복 및 에너지 초기화 완료!");
    }
}