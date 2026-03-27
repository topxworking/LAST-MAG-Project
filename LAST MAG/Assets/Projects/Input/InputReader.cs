using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "InputReader", menuName = "Game/Input Reader")]
public class InputReader : ScriptableObject, GameInputActions.IPlayerActions
{
    private GameInputActions _actions;

    public event Action<Vector2> OnMove;
    public event Action<Vector2> OnLook;
    public event Action OnJumpStarted;
    public event Action OnShootStarted;
    public event Action OnShootCanceled;
    public event Action OnAimStarted;
    public event Action OnAimCanceled;
    public event Action OnReloadStarted;
    public event Action OnPauseStarted;

    private void OnEnable()
    {
        if (_actions == null)
        {
            _actions = new GameInputActions();
            _actions.Player.SetCallbacks(this);
        }
        _actions.Player.Enable();
    }

    private void OnDisable()
    {
        _actions?.Player.Disable();
    }

    public void OnMovement(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
            OnMove?.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnLookAround(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
            OnLook?.Invoke(ctx.ReadValue<Vector2>());
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {
        if (ctx.started) OnJumpStarted?.Invoke();
    }

    public void OnShoot(InputAction.CallbackContext ctx)
    {
        if (ctx.started)  OnShootStarted?.Invoke();
        if (ctx.canceled) OnShootCanceled?.Invoke();
    }

    public void OnAim(InputAction.CallbackContext ctx)
    {
        if (ctx.started)  OnAimStarted?.Invoke();
        if (ctx.canceled) OnAimCanceled?.Invoke();
    }

    public void OnReload(InputAction.CallbackContext ctx)
    {
        if (ctx.started) OnReloadStarted?.Invoke();
    }

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (ctx.started) OnPauseStarted?.Invoke();
    }

    public void EnablePlayerInput()  => _actions?.Player.Enable();
    public void DisablePlayerInput() => _actions?.Player.Disable();
}
