using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Vector2 MovementInputVector { get; private set; }
    public Vector2 LookInputVector { get; private set; }
    public bool JumpPressed { get; private set; }

    public void OnMove(InputValue inputValue)
    {
        MovementInputVector = inputValue.Get<Vector2>();
    }

    public void OnLook(InputValue inputValue)
    {
        LookInputVector = inputValue.Get<Vector2>();
    }

    public void OnJump(InputValue inputValue)
    {
        if (inputValue.isPressed)
            JumpPressed = true;
    }

    public void ResetJumpFlag()
    {
        JumpPressed = false;
    }
}
