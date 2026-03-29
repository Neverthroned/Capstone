using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlamethrowerCollision : MonoBehaviour
{
    [Range(1, 30)]
    public int segmentCount = 8;
    public float segmentSpacing = 0.5f;
    public float minFollowSpeed = 3f;
    public float maxFollowSpeed = 20f;
    public float damageRadius = 0.4f;
    public float flameSpeed = 5f;
    public LayerMask playerLayer;

    private Vector3[] segmentPositions;
    private float[] segmentActivationTimes;
    private int _lastSegmentCount;
    private bool _damageActive = false;

    public float damagePerTick = 10f;

    void Awake()
    {
        InitSegments();
    }

    void InitSegments()
    {
        segmentPositions = new Vector3[segmentCount];
        segmentActivationTimes = new float[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            segmentPositions[i] = transform.position - transform.forward * (i * segmentSpacing);
            segmentActivationTimes[i] = float.MaxValue; // inactive until ResetActivation() is called
        }
        _lastSegmentCount = segmentCount;
    }

    // Called by controller when flamethrower fires
    public void ResetActivation()
    {
        for (int i = 0; i < segmentCount; i++)
        {
            segmentActivationTimes[i] = Time.time + (i * segmentSpacing) / flameSpeed;
        }
        _damageActive = true;
    }

    // Called by controller when flamethrower stops
    public void DisableDamage()
    {
        _damageActive = false;
        for (int i = 0; i < segmentCount; i++)
        {
            segmentActivationTimes[i] = float.MaxValue;
        }
    }

    // Update segment chain
    void Update()
    {
        if (segmentCount != _lastSegmentCount) InitSegments();

        // Tracking always runs regardless of _damageActive
        segmentPositions[0] = transform.position;
        for (int i = 1; i < segmentCount; i++)
        {
            Vector3 targetPos = segmentPositions[i - 1]
                              - transform.forward * segmentSpacing;

            float t = i / (float)(segmentCount - 1);
            float segmentFollowSpeed = Mathf.Lerp(maxFollowSpeed, minFollowSpeed, t);

            segmentPositions[i] = Vector3.Lerp(
                segmentPositions[i],
                targetPos,
                segmentFollowSpeed * Time.deltaTime
            );
        }

        // Damage only runs when active
        if (_damageActive) DealDamage();

        for (int i = 0; i < segmentPositions.Length; i++)
        {
            float t = i / (float)(segmentCount - 1);
            float radius = Mathf.Lerp(damageRadius, damageRadius * 0.3f, t);
            DrawDebugSphere(segmentPositions[i], radius);
        }

    DealDamage();

        for (int i = 0; i < segmentPositions.Length; i++)
        {
            float t = i / (float)(segmentCount - 1);
            float radius = Mathf.Lerp(damageRadius, damageRadius * 0.3f, t);
            DrawDebugSphere(segmentPositions[i], radius);
        }
    }

    // Collsion and damage
    void DealDamage()
    {
        for (int i = 0; i < segmentPositions.Length - 1; i++)
        {
            // Skip this segment until the flame has had time to reach it
            if (Time.time < segmentActivationTimes[i]) continue;

            float t = i / (float)(segmentCount - 1);
            float radius = Mathf.Lerp(damageRadius, damageRadius * 0.3f, t);

            Collider[] hits = Physics.OverlapCapsule(
                segmentPositions[i],
                segmentPositions[i + 1],
                radius,
                playerLayer
            );

            Debug.DrawLine(segmentPositions[i], segmentPositions[i + 1],
                hits.Length > 0 ? Color.red : Color.white);

            foreach (var hit in hits)
            {
                // Debug.Log($"Hit: {hit.gameObject.name}, parent: {hit.transform.root.name}");
                PlayerStats stats = hit.GetComponentInParent<PlayerStats>();
                // Debug.Log($"PlayerStats found: {stats != null}");
                if (stats != null)
                {
                    stats.TakeDamageWithCooldown(damagePerTick);
                }
            }
        }
    }

    // Debug stuff
    void DrawDebugSphere(Vector3 center, float radius)
    {
        int segments = 16;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * Mathf.PI * 2;
            float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2;

            Vector3 xz1 = center + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * radius;
            Vector3 xz2 = center + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * radius;
            Debug.DrawLine(xz1, xz2, Color.red);

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
            float t = i / (float)(segmentPositions.Length - 1);
            float radius = Mathf.Lerp(damageRadius, damageRadius * 0.3f, t);

            bool active = segmentActivationTimes != null && Time.time >= segmentActivationTimes[i];
            // Grey = not yet active, yellow->red = active
            Gizmos.color = active ? Color.Lerp(Color.yellow, Color.red, t) : Color.grey;
            Gizmos.DrawWireSphere(segmentPositions[i], radius);
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < segmentPositions.Length - 1; i++)
        {
            Gizmos.DrawLine(segmentPositions[i], segmentPositions[i + 1]);
        }
    }
}