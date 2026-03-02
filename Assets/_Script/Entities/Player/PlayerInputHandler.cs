using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement; // [필수 추가] 씬 이동 감지용
using System;

public class PlayerInputHandler : MonoBehaviour
{
    Player_Actions input;

    // 이벤트 정의
    public event Action<Vector2> OnMove;
    public event Action<bool> OnJump;
    public event Action<bool> OnAttack;
    public event Action OnDodge;
    public event Action OnSkill1;
    public event Action OnSkill2;
    public event Action OnMaximize;

    // [핵심] 상호작용 이벤트 (여기에 상점, 포탈 등이 주렁주렁 매달림)
    public event Action OnInteract;

    public event Action OnInventory;
    public event Action OnUseItem1;
    public event Action OnUseItem2;
    public event Action OnUseItem3;

    // UI 이벤트
    public event Action<Vector2> OnNavigate;
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnPrevTab;
    public event Action OnNextTab;
    public event Action OnCloseUI;
    public event Action OnSwitchZone;

    private void Awake()
    {
        input = new Player_Actions();

        // [중요] 플레이어는 씬이 바껴도 살아남아야 함
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // 씬이 로드될 때마다 호출될 함수 등록
        SceneManager.sceneLoaded += OnSceneLoaded;

        input.Player.Move.performed += MoveCtx;
        input.Player.Move.canceled += MoveCtx;
        input.Player.Jump.started += JumpCtx;
        input.Player.Jump.canceled += JumpCtx;
        input.Player.Attack.started += AttackCtx;
        input.Player.Attack.canceled += AttackCtx;
        input.Player.Dodge.performed += DodgeCtx;
        input.Player.Skill1.performed += Skill1Ctx;
        input.Player.Skill2.performed += Skill2Ctx;
        input.Player.Maximize.performed += MaximizeCtx;
        input.Player.Interact.performed += InteractCtx;
        input.Player.Inventory.performed += InventoryCtx;
        input.Player.UseItem1.performed += UseItem1Ctx;
        input.Player.UseItem2.performed += UseItem2Ctx;
        input.Player.UseItem3.performed += UseItem3Ctx;

        input.UI.Navigate.performed += NavigateCtx;
        input.UI.Submit.performed += SubmitCtx;
        input.UI.Cancel.performed += CancelCtx;
        input.UI.PrevTab.performed += PrevTabCtx;
        input.UI.NextTab.performed += NextTabCtx;
        input.UI.CloseUI.performed += CloseUICtx;
        input.UI.SwitchZone.performed += SwitchZoneCtx;

        input.Enable();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 등록 해제
        input.Disable();
    }

    // ==========================================
    // [핵심 해결책] 씬이 로드되면 과거의 인연(상점 연결)을 모두 끊어버림!
    // ==========================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // OnInteract에 연결된 모든 함수(Delegate)를 비워버림
        OnInteract = null;
        Debug.Log($"[InputHandler] 씬 이동 감지: 상호작용 연결 초기화 완료 ({scene.name})");
    }

    // --- 입력 모드 전환 ---
    public void OpenUI(bool isOpen)
    {
        if (isOpen)
        {
            input.Player.Disable();
            input.UI.Enable();
        }
        else
        {
            input.UI.Disable();
            input.Player.Enable();
        }
    }

    // --- 콜백 메서드들 ---
    private void MoveCtx(InputAction.CallbackContext ctx) => OnMove?.Invoke(ctx.ReadValue<Vector2>());
    private void JumpCtx(InputAction.CallbackContext ctx) => OnJump?.Invoke(ctx.ReadValueAsButton());
    private void AttackCtx(InputAction.CallbackContext ctx) => OnAttack?.Invoke(ctx.ReadValueAsButton());
    private void DodgeCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnDodge?.Invoke(); }
    private void Skill1Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSkill1?.Invoke(); }
    private void Skill2Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSkill2?.Invoke(); }
    private void MaximizeCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnMaximize?.Invoke(); }

    private void InteractCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Debug.Log("F키 입력 감지됨"); // 디버깅용 로그
            OnInteract?.Invoke();
        }
    }

    private void InventoryCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnInventory?.Invoke(); }
    private void UseItem1Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem1?.Invoke(); }
    private void UseItem2Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem2?.Invoke(); }
    private void UseItem3Ctx(InputAction.CallbackContext ctx) { if (ctx.performed) OnUseItem3?.Invoke(); }

    private void NavigateCtx(InputAction.CallbackContext ctx) => OnNavigate?.Invoke(ctx.ReadValue<Vector2>());
    private void SubmitCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSubmit?.Invoke(); }
    private void CancelCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnCancel?.Invoke(); }
    private void PrevTabCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnPrevTab?.Invoke(); }
    private void NextTabCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnNextTab?.Invoke(); }
    private void CloseUICtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnCloseUI?.Invoke(); }
    private void SwitchZoneCtx(InputAction.CallbackContext ctx) { if (ctx.performed) OnSwitchZone?.Invoke(); }
}