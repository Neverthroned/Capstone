using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraFollow : MonoBehaviour
{
    [Header("Sensitivity")]
    public float sensX = 200f;
    public float sensY = 200f;

    [Header("References")]
    public Transform orientation;
    public PlayerGravityManager gravityManager;

    private float xRotation;
    private float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Auto-assign if not set
        if (!gravityManager)
            gravityManager = Object.FindFirstObjectByType<PlayerGravityManager>();
    }

    private void Update()
    {
        // Mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Get the player's current "up" direction from gravity
        Vector3 playerUp = -gravityManager.CurrentGravityDir;

        // Align to gravity
        Quaternion gravityAligned = Quaternion.FromToRotation(Vector3.up, playerUp);

        // Apply yaw (around up) and pitch (around right)
        Quaternion yaw = Quaternion.AngleAxis(yRotation, playerUp);
        Quaternion pitch = Quaternion.AngleAxis(xRotation, orientation.right);

        // Combine rotations
        transform.rotation = yaw * gravityAligned * Quaternion.Euler(xRotation, 0, 0);

        // Update orientation for movement
        orientation.rotation = yaw * gravityAligned;
    }
}