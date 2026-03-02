using UnityEngine;
using System.Collections; // [필수] 코루틴 사용을 위해 추가

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
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // [중요] 플레이어 매니저도 씬 넘어갈 때 살아남아야 함!
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (startAsMecha) ToMecha(true);
        else ToHuman(true);
    }

    // ==========================================
    // [핵심 수정] 순간이동 후 콜라이더를 리셋해서 상호작용 재인식 시키기
    // ==========================================
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

        // [추가] 물리 충돌 재계산 강제 실행
        StartCoroutine(RefreshColliderRoutine());
    }

    private IEnumerator RefreshColliderRoutine()
    {
        // 현재 활성화된 캐릭터의 콜라이더를 찾음
        Collider2D activeCol = null;
        if (IsMechaMode && mechaObject != null) activeCol = mechaObject.GetComponent<Collider2D>();
        else if (humanObject != null) activeCol = humanObject.GetComponent<Collider2D>();

        if (activeCol != null)
        {
            // 잠깐 껐다가
            activeCol.enabled = false;
            // 한 프레임 대기 (물리 엔진이 '사라졌다'고 인식할 시간 줌)
            yield return null;
            // 다시 켬 (이제 '새로 나타났다'고 인식해서 OnTriggerEnter가 발동됨!)
            activeCol.enabled = true;
            Debug.Log("Warp 완료: 상호작용 트리거 강제 갱신됨");
        }
    }
    // ==========================================

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
        droneObject.SetActive(false);

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
        droneObject.SetActive(true);

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

    public void ResetPlayerStatsForVillage()
    {
        if (humanObject != null)
        {
            PlayerState pState = humanObject.GetComponent<PlayerState>();
            if (pState != null)
            {
                pState.currentHp = pState.FinalMaxHP;
                if (!IsMechaMode) UIManager.Instance?.UpdateHP(pState.currentHp, pState.FinalMaxHP);
            }
        }

        if (mechaObject != null)
        {
            MechaState mState = mechaObject.GetComponent<MechaState>();
            if (mState != null)
            {
                mState.currentHp = mState.FinalMaxHP;
                if (IsMechaMode) UIManager.Instance?.UpdateHP(mState.currentHp, mState.FinalMaxHP);
            }
        }

        if (PlayerSkillController.Instance != null)
        {
            PlayerSkillController.Instance.ResetMechaEnergy();
        }

        Debug.Log("마을 도착: 플레이어 체력 회복 및 에너지 초기화 완료!");
    }
}