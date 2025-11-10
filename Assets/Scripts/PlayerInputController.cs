using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public Vector2 MovementInputVector { get; private set; }
    public Vector2 LookInputVector { get; private set; }
    public bool JumpPressed { get; private set; }

    private float jumpInputBuffer = 0f;

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
        {
            JumpPressed = true;
            // Allow jump buffering for 0.25s
            jumpInputBuffer = 0.25f;
        }
    }

    public void ResetJumpFlag()
    {
        JumpPressed = false;
    }

    void Update()
    {
        jumpInputBuffer -= Time.deltaTime;
        if (jumpInputBuffer < 0)
        {
            ResetJumpFlag();
        }
    }
}
