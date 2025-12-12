using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float groundDrag = 5f;
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;
    bool readyToJump = true;

    [Header("Gravity")]
    public float gravityStrength = 9.81f;
    public Vector3 gravityDirection = Vector3.down; // can be changed dynamically!

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("References")]
    public Transform orientation; // controls player facing

    private Rigidbody rb;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    [HideInInspector]
    public bool disableInput = false; // true when movement should be blocked (e.g., grappling)

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false; // we'll handle gravity manually
    }

    private void Update()
    {
        // --- CUSTOM GRAVITY APPLICATION ---
        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);

        // --- GROUND CHECK (ALONG GRAVITY) ---
        RaycastHit hit;
        float rayDistance = playerHeight * 0.5f + 1f;

        if (Physics.Raycast(transform.position, gravityDirection, out hit, rayDistance))
        {
            // Check if the hit object has GroundSurface script
            if (hit.collider.GetComponent<GroundSurface>() != null)
            {
                grounded = true;

                // Update gravity based on surface normal
                Vector3 newGravity = -hit.normal;
                gravityDirection = Vector3.Lerp(gravityDirection, newGravity, Time.deltaTime * 10f);
            }
            else
            {
                grounded = false;
            }
        }
        else
        {
            grounded = false;
        }

        MyInput();
        SpeedControl();

        // --- DRAG CONTROL ---
        rb.drag = grounded ? groundDrag : 0f;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        if (disableInput) return; // skip input if disabled

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(KeyCode.Space) && readyToJump)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MovePlayer()
    {
        // Forward/right should be tangent to surface (not into it)
        Vector3 localUp = -gravityDirection.normalized;
        Vector3 forward = Vector3.ProjectOnPlane(orientation.forward, localUp).normalized;
        Vector3 right = Vector3.ProjectOnPlane(orientation.right, localUp).normalized;

        moveDirection = forward * verticalInput + right * horizontalInput;

        float multiplier = grounded ? 1f : airMultiplier;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * multiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        // Limit velocity on the plane tangent to surface
        Vector3 localUp = -gravityDirection.normalized;
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, localUp);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = limitedVel + Vector3.Project(rb.velocity, localUp);
        }
    }

    private void Jump()
    {
        Vector3 localUp = -gravityDirection.normalized;
        rb.velocity = Vector3.ProjectOnPlane(rb.velocity, localUp);
        rb.AddForce(localUp * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // Optional: helper for dynamic gravity updates
    public void SetGravityDirection(Vector3 newDirection)
    {
        Quaternion targetRot =
    Quaternion.FromToRotation(transform.up, -gravityDirection) * transform.rotation;

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }
}