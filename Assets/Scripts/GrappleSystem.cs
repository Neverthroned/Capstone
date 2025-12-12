using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerGravityManager))]
[RequireComponent(typeof(Movement))]
public class GrappleSystem : MonoBehaviour
{
    [Header("Grapple Settings")]
    public float grappleRange = 20f;       // Max distance to grapple
    public float pullSpeed = 15f;          // Speed moving toward grapple
    public float autoStopDistance = 0.5f;  // How close to stop grappling

    private PlayerGravityManager gravityManager;
    private Movement move;
    private Rigidbody rb;

    private bool isAiming = false;          // RMB is held
    private bool isGrappling = false;
    private Vector3 grapplePoint;
    private GrappleTarget targetSurface;

    private void Start()
    {
        gravityManager = GetComponent<PlayerGravityManager>();
        move = GetComponent<Movement>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Start aiming when holding RMB
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            isAiming = true;
            HighlightTarget();
        }

        // Release RMB to grapple
        if (isAiming && Input.GetMouseButtonUp(1))
        {
            if (targetSurface != null)
            {
                StartGrapple();
            }
            isAiming = false;
        }

        // Grappling movement
        if (isGrappling)
        {
            PullPlayer();
        }
    }

    // Raycast from camera to find valid grapple target
    private void HighlightTarget()
    {
        RaycastHit hit;
        Camera cam = Camera.main;
        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, grappleRange))
        {
            targetSurface = hit.collider.GetComponent<GrappleTarget>();
            if (targetSurface != null)
            {
                grapplePoint = hit.point;
                // Optional: add highlight visuals here
            }
            else
            {
                targetSurface = null;
            }
        }
        else
        {
            targetSurface = null;
        }
    }

    private void StartGrapple()
    {
        isGrappling = true;
        rb.velocity = Vector3.zero; // reset velocity for clean pull

        // Flip gravity toward surface normal
        gravityManager.InstantSetGravity(-targetSurface.transform.up);
        move.disableInput = true;
    }

    private void PullPlayer()
    {
        Vector3 direction = (grapplePoint - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, grapplePoint);

        rb.MovePosition(transform.position + direction * pullSpeed * Time.deltaTime);

        if (distance < autoStopDistance)
        {
            isGrappling = false;
            move.disableInput = false;
        }
    }
}