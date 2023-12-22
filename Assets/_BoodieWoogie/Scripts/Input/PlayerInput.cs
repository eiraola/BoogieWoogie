using UnityEngine;
using UnityEngine.InputSystem;
using System;
using static Input;
[CreateAssetMenu(fileName = "New Input Reader,", menuName = "Input/InputReader")]

public class PlayerInput : ScriptableObject ,IPlayerActions 
{
    private Input input;
    public event Action<Vector2> OnMoveEvent;
    public event Action OnJumpEvent;

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnJumpEvent?.Invoke();
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
    }
    private void OnEnable()
    {
        if (input == null)
        {
            input = new Input();
            input.Player.SetCallbacks(this);
        }
        input.Player.Enable();
    }
}
