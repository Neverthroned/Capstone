using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerCollision : MonoBehaviour
{
    public int segmentCount = 12;
    public float segmentSpacing = 0.5f;
    public float followSpeed = 15f;
    public float damageRadius = 0.4f;

    private Vector3[] segmentPositions;

    void Start()
    {
        segmentPositions = new Vector3[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segmentPositions[i] = transform.position;
        }

        Debug.Log($"FlamethrowerCollision Start() fired. Position: {transform.position}, Segments: {segmentCount}");
    }

    void Update()
    {
        Debug.DrawRay(transform.position, Vector3.up * 5f, Color.green, 0, false);

        segmentPositions[0] = transform.position;

        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 targetPos = segmentPositions[i - 1]
                              - transform.forward * segmentSpacing;

            segmentPositions[i] = Vector3.Lerp(
                segmentPositions[i],
                targetPos,
                followSpeed * Time.deltaTime
            );
        }

        // Draw in Scene view 
        for (int i = 0; i < segmentPositions.Length; i++)
        {
            DrawDebugSphere(segmentPositions[i], damageRadius);
        }
    }

    void DrawDebugSphere(Vector3 center, float radius)
    {
        int segments = 16; 

        // Draw 3 rings (XZ, XY, YZ planes) so it looks like a sphere
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * Mathf.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

            // XZ plane (horizontal ring)
            Vector3 xz1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 xz2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            Debug.DrawLine(xz1, xz2, Color.red);

            // XY plane (vertical ring)
            Vector3 xy1 = center + new Vector3(Mathf.Cos(angle1), Mathf.Sin(angle1), 0) * radius;
            Vector3 xy2 = center + new Vector3(Mathf.Cos(angle2), Mathf.Sin(angle2), 0) * radius;
            Debug.DrawLine(xy1, xy2, Color.yellow);
        }
    }

    void OnDrawGizmos()
    {
        if (segmentPositions == null) return;

        for (int i = 0; i < segmentPositions.Length; i++)
        {
            // Color shifts from orange (muzzle) to red (tail) for easier reading
            float t = i / (float)(segmentPositions.Length - 1);
            Gizmos.color = Color.Lerp(Color.yellow, Color.red, t);
            Gizmos.DrawWireSphere(segmentPositions[i], damageRadius);
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < segmentPositions.Length - 1; i++)
        {
            Gizmos.DrawLine(segmentPositions[i], segmentPositions[i + 1]);
        }
    }
}