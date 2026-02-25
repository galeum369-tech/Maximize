using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    Player_Actions input;

    // 이벤트
    // 플레이어 이벤트
    public event Action<Vector2> OnMove;
    public event Action<bool> OnJump;    // [수정] bool 값 전달 (true: 누름, false: 뗌)
    public event Action<bool> OnAttack;  // [수정] bool 값 전달 (true: 누름, false: 뗌)
    public event Action OnDodge;
    public event Action OnSkill1;
    public event Action OnSkill2;
    public event Action OnMaximize;
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
    }

    private void OnEnable()
    {
        // 플레이어 
        input.Player.Move.performed += MoveCtx;
        input.Player.Move.canceled += MoveCtx;

        // [수정] 점프: 누름(performed)과 뗌(canceled) 모두 구독
        input.Player.Jump.performed += JumpCtx;
        input.Player.Jump.canceled += JumpCtx;

        // [수정] 공격: 누름(performed)과 뗌(canceled) 모두 구독
        input.Player.Attack.performed += AttackCtx;
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

        // UI
        input.UI.Navigate.performed += NavigateCtx;
        input.UI.Navigate.canceled += NavigateCtx;
        input.UI.Submit.performed += SubmitCtx;
        input.UI.Cancel.performed += CancelCtx;
        input.UI.PrevTab.performed += PrevTabCtx;
        input.UI.NextTab.performed += NextTabCtx;
        input.UI.CloseUI.performed += CloseUICtx;

        input.UI.SwitchZone.performed += SwitchZoneCtx;

        input.Player.Enable();
        input.UI.Disable();
    }

    private void OnDisable()
    {
        // 플레이어 
        input.Player.Move.performed -= MoveCtx;
        input.Player.Move.canceled -= MoveCtx;

        // [수정] 점프 구독 해제
        input.Player.Jump.performed -= JumpCtx;
        input.Player.Jump.canceled -= JumpCtx;

        // [수정] 공격 구독 해제
        input.Player.Attack.performed -= AttackCtx;
        input.Player.Attack.canceled -= AttackCtx;

        input.Player.Dodge.performed -= DodgeCtx;
        input.Player.Skill1.performed -= Skill1Ctx;
        input.Player.Skill2.performed -= Skill2Ctx;
        input.Player.Maximize.performed -= MaximizeCtx;
        input.Player.Interact.performed -= InteractCtx;
        input.Player.Inventory.performed -= InventoryCtx;
        input.Player.UseItem1.performed -= UseItem1Ctx;
        input.Player.UseItem2.performed -= UseItem2Ctx;
        input.Player.UseItem3.performed -= UseItem3Ctx;

        // UI
        input.UI.Navigate.performed -= NavigateCtx;
        input.UI.Navigate.canceled -= NavigateCtx;
        input.UI.Submit.performed -= SubmitCtx;
        input.UI.Cancel.performed -= CancelCtx;
        input.UI.PrevTab.performed -= PrevTabCtx;
        input.UI.NextTab.performed -= NextTabCtx;
        input.UI.CloseUI.performed -= CloseUICtx;

        input.UI.SwitchZone.performed -= SwitchZoneCtx;

        input.Disable();
    }

    public void OpenUI(bool isOpen)
    {
        if (isOpen)
        {
            input.Player.Disable();
            input.UI.Enable();
            print("UI Opened");
        }
        else
        {
            input.Player.Enable();
            input.UI.Disable();
            print("UI Closed");
        }
    }

    #region 콜백 메서드

    private void MoveCtx(InputAction.CallbackContext ctx)
    {
        Vector2 inputVector = ctx.ReadValue<Vector2>();
        OnMove?.Invoke(inputVector);
    }

    private void JumpCtx(InputAction.CallbackContext ctx)
    {
        // [수정] ctx.performed가 true면 누름, false면 뗌 상태 전달
        OnJump?.Invoke(ctx.performed);
    }

    private void AttackCtx(InputAction.CallbackContext ctx)
    {
        // [수정] ctx.performed가 true면 누름, false면 뗌 상태 전달
        OnAttack?.Invoke(ctx.performed);
    }

    private void DodgeCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnDodge?.Invoke();
        }
    }

    private void Skill1Ctx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnSkill1?.Invoke();
    }

    private void Skill2Ctx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnSkill2?.Invoke();
    }

    private void MaximizeCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnMaximize?.Invoke();
    }

    private void InteractCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnInteract?.Invoke();
    }

    private void InventoryCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnInventory?.Invoke();
    }

    private void UseItem1Ctx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnUseItem1?.Invoke();
    }

    private void UseItem2Ctx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnUseItem2?.Invoke();
    }

    private void UseItem3Ctx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnUseItem3?.Invoke();
    }

    // --- [UI Map] 콜백 메서드 ---

    private void NavigateCtx(InputAction.CallbackContext ctx)
    {
        // 이제 에셋에서 Composite로 묶어줬기 때문에 Vector2를 안전하게 읽어올 수 있어!
        Vector2 navVector = ctx.ReadValue<Vector2>();
        OnNavigate?.Invoke(navVector);
    }

    private void SubmitCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnSubmit?.Invoke();
    }

    private void CancelCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnCancel?.Invoke();
    }

    private void PrevTabCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnPrevTab?.Invoke();
    }

    private void NextTabCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnNextTab?.Invoke();
    }

    private void CloseUICtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) OnCloseUI?.Invoke();
    }

    private void SwitchZoneCtx(InputAction.CallbackContext ctx)
    { 
        if (ctx.performed) OnSwitchZone?.Invoke(); 
    }
    #endregion
}