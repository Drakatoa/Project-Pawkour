using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInputController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float inputAccel = 10f;
    [SerializeField] private float maxMoveSpeed = 100f;
    [SerializeField] private float wallRunJumpCoefficient = 0.1f;
    [SerializeField] private float movementDecel = 5f;
    [SerializeField] private float jumpDecel = 15f;
    [SerializeField] private float airborneTargetVelocity = 20f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private Transform cameraTransform;

    private CharacterController controller;
    private PlayerInputController input;
    private Vector3 velocity;

    private bool isWallRunning = false;

    private Collider prevWall = null;

    private LayerMask wallRunMask;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputController>();
        wallRunMask = LayerMask.GetMask("Wall Run");
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
            float multiplier = horizontalV.magnitude / airborneTargetVelocity * inputAccel / jumpDecel;
            horizontalV = horizontalV.normalized * Mathf.Max(0, horizontalV.magnitude - jumpDecel * multiplier * Time.deltaTime);
        }

        velocity = new Vector3(horizontalV.x, velocity.y, horizontalV.y);
        
        Collider[] collisions = Physics.OverlapCapsule(transform.position + Vector3.up * controller.height / 2, transform.position - Vector3.up * controller.height / 2,
                                    controller.radius + 0.1f, wallRunMask);

        // Wall Running
        if (!isWallRunning && !isGrounded)
        {
            if (collisions.Length > 0 && horizontalV.magnitude > 15f && prevWall != collisions[0])
            {
                prevWall = collisions[0];
                isWallRunning = true;
                velocity.y = Mathf.Sqrt(-gravity) * jumpHeight + Mathf.Max(0, velocity.y) * wallRunJumpCoefficient;
            }
        }

        if(isWallRunning && collisions.Length < 1)
        {
            isWallRunning = false;
        }
        
        // Wall Run Jump
        if(isWallRunning && input.JumpPressed)
        {
            Vector3 collisionPoint = Physics.ClosestPoint(transform.position, collisions[0], collisions[0].transform.position, collisions[0].transform.rotation);
            Vector3 normal = (transform.position - collisionPoint).normalized;
            RaycastHit hit;
            if (Physics.Raycast(collisionPoint, collisionPoint - transform.position, out hit, 5f, wallRunMask))
            {
                // Unsure if needed
            }
            velocity = normal * jumpHeight * 2 + Vector3.ProjectOnPlane(velocity, normal);
            velocity.y = Mathf.Sqrt(-jumpHeight * gravity);
            input.ResetJumpFlag();
            isWallRunning = false;
        }

        if(isGrounded)
        {
            prevWall = null;
            isWallRunning = false;
        }

        // Ground check
        if (isGrounded && velocity.y < 0)
            velocity.y = 0f;

        // Normal Jump
        if (input.JumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -jumpHeight * gravity);
            input.ResetJumpFlag();
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        if (velocity.magnitude > maxMoveSpeed)
        {
            velocity = velocity.normalized * maxMoveSpeed;
        }
        controller.Move(velocity * Time.deltaTime);

        if (Vector3.Dot(input.MovementInputVector, Vector2.up) > 0)
        {
            Vector3 fNormal = Vector3.ProjectOnPlane(f * velocity.magnitude, Vector3.up) + Vector3.Project(velocity, Vector3.down);
            velocity = Vector3.Lerp(velocity, fNormal, Time.deltaTime);
        }

    }
}
