using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float inputAccel = 10f;
    [SerializeField] private float maxMoveSpeed = 100f;
    [SerializeField] private float movementDecel = 5f;
    [SerializeField] private float jumpDecel = 15f;
    [SerializeField] private float airborneTargetVelocity = 20f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private PlayerInputController input;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputController>();
    }

    void Update()
    {
        // Do all Horizontal movement first, then calculate any vertical movement.
        bool isGrounded = controller.isGrounded;

        // Calculate Horizontal Velocity
        Vector2 horizontalV = new Vector2(velocity.x, velocity.z);

        // Apply movementDecel
        horizontalV = horizontalV.normalized * Mathf.Max(0, horizontalV.magnitude - movementDecel * Time.deltaTime);

        // Movement relative to camera
        Vector3 f = cameraTransform.forward;
        Vector3 r = cameraTransform.right;
        Vector2 forward = new Vector2(f.x, f.z).normalized;
        Vector2 right = new Vector2(r.x, r.z).normalized;

        // Calculate deltaVelocity from input and inputAccel
        Vector2 deltaVelocity = (right * input.MovementInputVector.x + forward * input.MovementInputVector.y) * inputAccel * Time.deltaTime;

        horizontalV += deltaVelocity;

        //JumpDecel
        if (!isGrounded)
        {
            // The faster you are going, the more decel, and when you are AT the target, they should cancel each other out.
            float multiplier = horizontalV.magnitude/airborneTargetVelocity * inputAccel/jumpDecel;
            horizontalV = horizontalV.normalized * Mathf.Max(0, horizontalV.magnitude - jumpDecel * multiplier * Time.deltaTime);
        }

        Debug.Log(horizontalV);

        velocity = new Vector3(horizontalV.x, velocity.y, horizontalV.y);

        // Ground check
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;
        
        // Jump
        if (input.JumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -jumpHeight * gravity);
            input.ResetJumpFlag();
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        if(velocity.magnitude > maxMoveSpeed)
        {
            velocity = velocity.normalized * maxMoveSpeed;
        }
        controller.Move(velocity * Time.deltaTime);
        Debug.Log(velocity);
    }
}
