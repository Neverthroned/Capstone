using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleProjectile : MonoBehaviour
{
    [Header("Homing")]
    public GameObject target;
    public float speed = 1.0f;
    public float scaleUpDuration = 0.5f;
    public float maxScale = 1.0f;

    [Header("Gravity")]
    public float gravityRadius = 8f;
    public float gravityStrength = 2f;
    public float mergeDistance = 0.5f;

    [Header("Merge")]
    public GameObject whiteHolePrefab;

    private bool isActive = false;
    private bool hasMerged = false;         // Guard against double merge
    private Vector3 velocity;              // Track velocity for white hole inheritance
    private Coroutine homingCoroutine;

    public void Launch()
    {
        isActive = true;
        homingCoroutine = StartCoroutine(HomingSequence());
    }

    private void Update()
    {
        if (!isActive) return;
        ApplyGravityToNearby();
    }

    private void ApplyGravityToNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, gravityRadius);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            // Clamp pull so it can never exceed homing speed. prevents slingshot effect
            float gravitationalPull = Mathf.Min(
                gravityStrength / Mathf.Max(distance * distance, 0.1f),
                speed * 0.8f
            );

            hit.transform.position += (transform.position - hit.transform.position).normalized * gravitationalPull * Time.deltaTime;

            if (distance <= mergeDistance)
            {
                BlackHoleProjectile otherBlackHole = hit.GetComponent<BlackHoleProjectile>();

                if (otherBlackHole != null && !hasMerged && !otherBlackHole.hasMerged)
                {
                    hasMerged = true;
                    otherBlackHole.hasMerged = true;

                    Vector3 midpoint = (transform.position + hit.transform.position) / 2f;
                    Vector3 combinedVelocity = velocity + otherBlackHole.velocity;

                    GameObject whiteHole = Instantiate(whiteHolePrefab, midpoint, Quaternion.identity);
                    WhiteHole whiteHoleScript = whiteHole.GetComponent<WhiteHole>();
                    whiteHoleScript.scaleUpDuration = scaleUpDuration;
                    whiteHoleScript.maxScale = maxScale;
                    whiteHoleScript.Launch(combinedVelocity);

                    otherBlackHole.DestroyBlackHole();
                    DestroyBlackHole();
                    return;
                }
                else if (otherBlackHole == null)
                {
                    Destroy(hit.gameObject);
                }
            }
        }
    }

    public IEnumerator HomingSequence()
    {
        float elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.one * (easedT * maxScale);
            MoveTowardTarget();
            yield return null;
        }

        transform.localScale = Vector3.one * maxScale;

        while (target != null && Vector3.Distance(target.transform.position, transform.position) > 0.3f)
        {
            MoveTowardTarget();
            yield return null;
        }

        DestroyBlackHole();
    }

    private void MoveTowardTarget()
    {
        if (target == null) return;
        Vector3 direction = (target.transform.position - transform.position).normalized;
        velocity = direction * speed;                       // Keep velocity in sync
        transform.position += velocity * Time.deltaTime;
        transform.LookAt(target.transform);
    }

    public void DestroyBlackHole()
    {
        isActive = false;
        if (homingCoroutine != null) StopCoroutine(homingCoroutine);
        Destroy(gameObject);
    }
}