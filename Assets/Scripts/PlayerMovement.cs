using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float groundDrag = 5f;

    [Header("References")]
    public Transform orientation; // player orientation and facing direction

    private Rigidbody rb;
    float verticalMoveInput;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 1.5f;        // global drag
        rb.angularDrag = 2f;
    }

    private void Update()
    {
        MyInput();

    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (rb.velocity.magnitude > moveSpeed)
            rb.velocity = rb.velocity.normalized * moveSpeed; // Prevents endless acceleration
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A / D
        verticalInput = Input.GetAxisRaw("Vertical");     // W / S

        verticalMoveInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalMoveInput += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) verticalMoveInput -= 1f;
    }

    private void MovePlayer()
    {
        Vector3 moveDirection =
            orientation.forward * verticalInput +
            orientation.right * horizontalInput +
            orientation.up * verticalMoveInput;

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Acceleration);
        }
    }
}