using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputHandler : MonoBehaviour
{
    Player_Actions input;

    //이벤트
    //플레이어 이벤트
    public event Action<Vector2> OnMove;
    public event Action OnJump;
    public event Action OnAttack;
    public event Action OnDodge;
    public event Action OnSkill1;
    public event Action OnSkill2;
    public event Action OnMaximize;
    public event Action OnInteract;
    public event Action OnInventory;
    public event Action OnUseItem1;
    public event Action OnUseItem2;
    public event Action OnUseItem3;

    //UI 이벤트
    public event Action<Vector2> OnNavigate;
    public event Action OnSubmit;
    public event Action OnCancel;
    public event Action OnPrevTab;
    public event Action OnNextTab;
    public event Action OnCloseUI;

    private void Awake()
    {
        input = new Player_Actions();
    }

    private void OnEnable()
    {
        //플레이어 
        input.Player.Move.performed += MoveCtx;
        input.Player.Move.canceled += MoveCtx;
        input.Player.Jump.performed += JumpCtx;
        input.Player.Attack.performed += AttackCtx;
        input.Player.Dodge.performed += DodgeCtx;
        input.Player.Skill1.performed += Skill1Ctx;
        input.Player.Skill2.performed += Skill2Ctx;
        input.Player.Maximize.performed += MaximizeCtx;
        input.Player.Interact.performed += InteractCtx;
        input.Player.Inventory.performed += InventoryCtx;
        input.Player.UseItem1.performed += UseItem1Ctx;
        input.Player.UseItem2.performed += UseItem2Ctx;
        input.Player.UseItem3.performed += UseItem3Ctx;

        //UI
        input.UI.Navigate.performed += NavigateCtx;
        input.UI.Navigate.canceled += NavigateCtx;
        input.UI.Submit.performed += SubmitCtx;
        input.UI.Cancel.performed += CancelCtx;
        input.UI.PrevTab.performed += PrevTabCtx;
        input.UI.NextTab.performed += NextTabCtx;
        input.UI.CloseUI.performed += CloseUICtx;


        input.Player.Enable();
        input.UI.Disable();
    }

    private void OnDisable()
    {
        //플레이어 
        input.Player.Move.performed -= MoveCtx;
        input.Player.Move.canceled -= MoveCtx;
        input.Player.Jump.performed -= JumpCtx;
        input.Player.Attack.performed -= AttackCtx;
        input.Player.Dodge.performed -= DodgeCtx;
        input.Player.Skill1.performed -= Skill1Ctx;
        input.Player.Skill2.performed -= Skill2Ctx;
        input.Player.Maximize.performed -= MaximizeCtx;
        input.Player.Interact.performed -= InteractCtx;
        input.Player.Inventory.performed -= InventoryCtx;
        input.Player.UseItem1.performed -= UseItem1Ctx;
        input.Player.UseItem2.performed -= UseItem2Ctx;
        input.Player.UseItem3.performed -= UseItem3Ctx;

        //UI
        input.UI.Navigate.performed -= NavigateCtx;
        input.UI.Navigate.canceled -= NavigateCtx;
        input.UI.Submit.performed -= SubmitCtx;
        input.UI.Cancel.performed -= CancelCtx;
        input.UI.PrevTab.performed -= PrevTabCtx;
        input.UI.NextTab.performed -= NextTabCtx;
        input.UI.CloseUI.performed -= CloseUICtx;


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
        // Vector2 값을 읽어서 이동 이벤트 발생 [cite: 2026-02-12]
        Vector2 inputVector = ctx.ReadValue<Vector2>();
        OnMove?.Invoke(inputVector);
    }

    private void JumpCtx(InputAction.CallbackContext ctx)
    {
        // 버튼이 눌린 순간(Performed)에만 이벤트 발생 [cite: 2026-02-12]
        if (ctx.performed)
        {
            OnJump?.Invoke();
        }
    }

    private void AttackCtx(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            OnAttack?.Invoke();
        }
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
        // UI 선택창 이동 값 전달
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
    #endregion
}


