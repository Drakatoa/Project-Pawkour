using UnityEngine;

[RequireComponent(typeof(PlayerInputController))]
public class CameraLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private CharacterController player;
    [SerializeField] private float mouseSensitivity = 100f;

    [SerializeField] private float followRatio = 0.7f;
    [SerializeField] private float rotRatio = 0.7f;
    [SerializeField] private float horizontalRotationSensitivity = 0.2f;
    private PlayerInputController input;

    private float cameraHeight = 2.0f;

    private float xAccumulatedAngle = 0f;

    void Awake()
    {
        input = playerBody.GetComponent<PlayerInputController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        Vector2 look = input.LookInputVector;
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        Vector3 movementDirection = Vector3.Scale(new Vector3(1, 0, 1), playerBody.position - transform.position);

        xAccumulatedAngle += mouseX;

        Vector3 lookDirection = playerBody.position - transform.position;

        cameraHeight += mouseY * horizontalRotationSensitivity;
        cameraHeight = Mathf.Clamp(cameraHeight, -1, 4);

        Vector3 targetPos = playerBody.position - movementDirection.normalized * 6 + Vector3.up * cameraHeight;

        transform.position = Vector3.Lerp(transform.position, targetPos, followRatio * Mathf.Max(player.velocity.magnitude, 1) * Time.deltaTime);

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotRatio * 20 * Time.deltaTime);

        transform.RotateAround(playerBody.position, Vector3.up, xAccumulatedAngle * rotRatio * horizontalRotationSensitivity);
        xAccumulatedAngle *= 1 - rotRatio * horizontalRotationSensitivity;
    }
}
