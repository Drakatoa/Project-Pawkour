using System;
using System.Collections.Generic;
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

    private LayerMask playerMask;

    void Awake()
    {
        input = playerBody.GetComponent<PlayerInputController>();
        Cursor.lockState = CursorLockMode.Locked;
        playerMask = LayerMask.GetMask("Player");
    }

    void LateUpdate()
    {
        Vector3 curPos = transform.position;
        Quaternion curRot = transform.rotation;

        Vector2 look = input.LookInputVector;
        float mouseX = look.x * mouseSensitivity * Time.deltaTime;
        float mouseY = look.y * mouseSensitivity * Time.deltaTime;

        xAccumulatedAngle += mouseX;

        transform.RotateAround(playerBody.position, Vector3.up, xAccumulatedAngle * rotRatio * horizontalRotationSensitivity);
        xAccumulatedAngle *= 1 - rotRatio * horizontalRotationSensitivity;

        Vector3 movementDirection = Vector3.Scale(new Vector3(1, 0, 1), playerBody.position - transform.position);

        cameraHeight += mouseY * horizontalRotationSensitivity;
        cameraHeight = Mathf.Clamp(cameraHeight, 0.4f, 4);

        Vector3 targetPos = playerBody.position - movementDirection.normalized * 6 + Vector3.up * cameraHeight;

        //RaycastHit hit;
        // Vector3 playerHit = Vector3.zero;
        // List<Vector3> positions = new List<Vector3>
        // {
        //     targetPos
        // };
        // if(Physics.Raycast(targetPos, playerBody.position - targetPos, out hit, 50f, playerMask))
        // {
        //     playerHit = hit.point;
        // }
        // while (Physics.Raycast(targetPos - (playerHit - targetPos).normalized * 0.3f, (playerHit - targetPos).normalized, out hit, (playerHit - targetPos).magnitude + 0.2f, ~playerMask))
        // {
        //     //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        //     transform.position = hit.point + (playerHit-targetPos).normalized * 0.5f;
        //     targetPos = transform.position;
        //     positions.Add(targetPos);
        // }

        // if(positions.Count > 1)
        // {
        //     Debug.Log(curPos + " " + String.Join(", ", positions));
        // }

        Vector3 lookDirection = playerBody.position - transform.position;

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), rotRatio * 20 * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPos, followRatio * Mathf.Max(player.velocity.magnitude, 1) * Time.deltaTime);

        // while (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, (playerBody.position - transform.position).magnitude - 1f))
        // {
        //     Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
        //     transform.position = hit.point + transform.TransformDirection(Vector3.forward).normalized * 0.5f;
        // }
    }
}
