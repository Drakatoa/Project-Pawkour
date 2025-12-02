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
    [SerializeField] private float slidingDecel = 2f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -22f;
    [SerializeField] private Transform cameraTransform;
    private struct LookWeight
        {
            public float weight;
            public float body;
            public float head;
            public float eyes;

            public LookWeight(float weight, float body, float head, float eyes)
            {
                this.weight = weight;
                this.body = body;
                this.head = head;
                this.eyes = eyes;
            }
        }

    private CharacterController controller;
    
    private CapsuleCollider collid;

    private PlayerInputController input;

    private Animator animator;

    private AnimationHandler animate;
    private Vector3 velocity;

    private bool isWallRunning = false;

    private float wallRotation = 0f;

    private bool isSliding = false;

    private Collider prevWall = null;

    private LayerMask wallRunMask;

    private LookWeight lw;

    private Transform rig;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputController>();
        animator = GetComponent<Animator>();
        animate = new AnimationHandler(animator, "Vert", "State");
        wallRunMask = LayerMask.GetMask("Wall Run");
        lw = new LookWeight(0.8f, 0.8f, 0.8f, 0.8f);
        rig = transform.Find("Kitty_001_rig").Find("Root");
        collid = GetComponent<CapsuleCollider>();
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

        // Apply Slide Decel
        if(input.CrouchPressed && isGrounded)
        {
            horizontalV = horizontalV.normalized * Mathf.Max(0, horizontalV.magnitude - movementDecel * Time.deltaTime);
        }

        //JumpDecel
        if (!isGrounded)
        {
            // The faster you are going, the more decel, and when you are AT the target, they should cancel each other out.
            float multiplier = horizontalV.magnitude / airborneTargetVelocity * inputAccel / jumpDecel;
            horizontalV = horizontalV.normalized * Mathf.Max(0, horizontalV.magnitude - jumpDecel * multiplier * Time.deltaTime);
        }

        velocity = new Vector3(horizontalV.x, velocity.y, horizontalV.y);
        
        Collider[] collisions = Physics.OverlapCapsule(transform.position + Vector3.up * collid.radius, transform.position - Vector3.up * collid.height,
                                    collid.height + 0.1f, wallRunMask);

        // Wall Running
        if (!isWallRunning && !isGrounded)
        {
            if (collisions.Length > 0 && horizontalV.magnitude > 15f && prevWall != collisions[0])
            {
                prevWall = collisions[0];
                isWallRunning = true;
                velocity.y = Mathf.Sqrt(-gravity) * jumpHeight + Mathf.Max(0, velocity.y) * wallRunJumpCoefficient;
                Vector3 collisionPoint = Physics.ClosestPoint(prevWall.transform.position, collid, transform.position, transform.rotation);
                Vector3 toPos = collisionPoint - transform.position;
                Debug.Log(collisionPoint);
                Debug.Log(transform.position);
                Debug.DrawRay(transform.position, collisionPoint - transform.position, Color.red, 20f);
                Vector3 bwd = transform.rotation * -transform.forward;
                Debug.DrawRay(transform.position, bwd * 2, Color.azure, 20f);
                bool isRight = Vector2.Dot(new Vector2(bwd.x, bwd.z), new Vector2(toPos.x, toPos.z)) >= 0;
                wallRotation = (isRight ? 1f : -1f) * 90f;
            }
        }

        if(isWallRunning && collisions.Length < 1)
        {
            isWallRunning = false;
            wallRotation = 0f;
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
            wallRotation = 0f;
        }

        if(isGrounded && isWallRunning)
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

        // Animation
        animate.Animate(horizontalV, (velocity.magnitude > 2f) ? 1f : 0f, Time.deltaTime);
        if(velocity.magnitude > 2f) {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(velocity) * Quaternion.AngleAxis(wallRotation, Vector3.Scale(transform.right, new Vector3(1,0,1))), 300 * Time.deltaTime);
        }
    }

    private void OnAnimatorIK()
    {
        animate.AnimateIK(transform.position, lw);
    }

    #region Handlers
    private class AnimationHandler
        {
            private readonly Animator m_Animator;
            private readonly string m_VerticalID;
            private readonly string m_StateID;

            private readonly float k_InputFlow = 4.5f;

            private float m_FlowState;
            private Vector2 m_FlowAxis;

            public AnimationHandler(Animator animator, string verticalID, string stateID)
            {
                m_Animator = animator;
                m_VerticalID = verticalID;
                m_StateID = stateID;
            }

            public void Animate(in Vector2 axis, float state, float deltaTime)
            {
                m_Animator.SetFloat(m_VerticalID, m_FlowAxis.magnitude);
                m_Animator.SetFloat(m_StateID, Mathf.Clamp01(m_FlowState));

                m_FlowAxis = Vector2.ClampMagnitude(m_FlowAxis + k_InputFlow * deltaTime * (axis - m_FlowAxis).normalized, 1f);
                m_FlowState = Mathf.Clamp01(m_FlowState + k_InputFlow * deltaTime * Mathf.Sign(state - m_FlowState));
            }

            public void AnimateIK(in Vector3 target, in LookWeight lookWeight)
            {
                m_Animator.SetLookAtPosition(target);
                m_Animator.SetLookAtWeight(lookWeight.weight, lookWeight.body, lookWeight.head, lookWeight.eyes);
            }
        }
        #endregion
}
