using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGravityManager : MonoBehaviour
{
    [Header("Gravity Settings")]
    public float gravityLerpSpeed = 5f;          // how quickly gravity blends to new direction
    public float groundCheckDistance = 3f;       // raycast length to find ground surface
    public Transform gravityProbe;               // optional: small offset transform for ray origin

    private Movement move;
    private Vector3 currentGravityDir;
    private Vector3 targetGravityDir;

    public Vector3 CurrentGravityDir => currentGravityDir;

    public bool isGrounded { get; private set; }
    public RaycastHit groundHit { get; private set; }

    private void Start()
    {
        move = GetComponent<Movement>();
        currentGravityDir = move.gravityDirection.normalized;
        targetGravityDir = currentGravityDir;
    }

    private void FixedUpdate()
    {
        UpdateSurfaceGravity();
        SmoothGravityUpdate();
    }

    private void UpdateSurfaceGravity()
    {
        Vector3 origin = gravityProbe ? gravityProbe.position : transform.position;
        RaycastHit hit;

        if (Physics.Raycast(origin, currentGravityDir, out hit, groundCheckDistance))
        {
            groundHit = hit;
            isGrounded = true;

            if (hit.collider.GetComponent<GroundSurface>())
            {
                targetGravityDir = -hit.normal.normalized;
            }
        }
        else
        {
            isGrounded = false;
            groundHit = new RaycastHit();
            targetGravityDir = currentGravityDir; // Keep existing gravity
        }
    }

    private void SmoothGravityUpdate()
    {
        // Smoothly interpolate toward target
        currentGravityDir = Vector3.Slerp(currentGravityDir, targetGravityDir, gravityLerpSpeed * Time.fixedDeltaTime);

        // Update the movement component with new gravity
        move.SetGravityDirection(currentGravityDir);
    }

    /// <summary>
    /// Instantly sets gravity (used for grappling or teleport flips)
    /// </summary>
    public void InstantSetGravity(Vector3 newGravityDir)
    {
        currentGravityDir = newGravityDir.normalized;
        targetGravityDir = newGravityDir.normalized;
        move.SetGravityDirection(newGravityDir.normalized);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + currentGravityDir * 2f);
        }
    }
#endif
}