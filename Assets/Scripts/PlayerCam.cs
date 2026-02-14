using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public float rollSpeed = 120f;

    public Transform orientation;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Yaw (around local up)
        transform.Rotate(Vector3.up, mouseX, Space.Self);

        // Pitch (around local right)
        transform.Rotate(Vector3.right, -mouseY, Space.Self);

        // Roll (around local forward)
        float rollInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rollInput += 1f;
        if (Input.GetKey(KeyCode.E)) rollInput -= 1f;

        transform.Rotate(Vector3.forward, rollInput * rollSpeed * Time.deltaTime, Space.Self);

        // Keep movement orientation synced
        orientation.rotation = transform.rotation;
    }
}