using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public float sensX = 200f;
    public float sensY = 200f;
    public float rollSpeed = 90f;

    public Transform orientation;

    float yaw;
    float pitch;
    float roll;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensY * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        if (Input.GetKey(KeyCode.Q)) roll += rollSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) roll -= rollSpeed * Time.deltaTime;

        Quaternion rotation = Quaternion.Euler(pitch, yaw, roll);

        transform.rotation = rotation;
        orientation.rotation = rotation;
    }
}