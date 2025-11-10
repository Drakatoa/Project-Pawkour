using UnityEngine;

[RequireComponent(typeof(PlayerInputController))]
public class CameraLook : MonoBehaviour
{
    [SerializeField] private Transform playerBody;
    [SerializeField] private CharacterController player;
    [SerializeField] private float mouseSensitivity = 100f;

    [SerializeField] private float followRatio = 0.7f;
    [SerializeField] private float rotRatio = 0.7f;

    [SerializeField] private float ROTATION_CONSTANT = 4000f;

    private Vector3 velocity = new Vector3(0,0,0);
    private PlayerInputController input;

    private float prevMouseX = 0.0f;

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

        Vector3 lookDirection = Vector3.Scale(new Vector3(1, 0, 1), playerBody.position - transform.position);

        Vector3 targetPos = playerBody.position - lookDirection.normalized * 6 + Vector3.up * 2;

        transform.position = Vector3.Lerp(transform.position, targetPos, followRatio * Mathf.Max(player.velocity.magnitude, 1) * Time.deltaTime);
        if(mouseX < 0.005)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), ROTATION_CONSTANT * rotRatio / player.velocity.magnitude * Time.deltaTime);

        transform.RotateAround(playerBody.position, Vector3.up, Mathf.Lerp(prevMouseX, mouseX, 2 * Time.deltaTime));
        // transform.RotateAround(playerBody.position, Quaternion.Euler(0,-90,0) * lookDirection, mouseY);

        prevMouseX = mouseX;
    }
}
