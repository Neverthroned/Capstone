using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleProjectile : MonoBehaviour
{
    [Header("Homing")]
    public GameObject target;
    public float speed = 6f;
    public float scaleUpDuration = 0.5f;
    public float maxScale = 1.0f;

    [Header("Gravity")]
    public float gravityRadius = 8f;
    public float gravityStrength = 15f;
    public float mergeDistance = 0.5f;

    [Header("Merge")]
    public GameObject whiteHolePrefab;

    [Header("Damage")]
    public float damageAmount = 10f;

    [SerializeField] private LayerMask affectedLayers;
    [SerializeField] private LayerMask damageLayers;

    private PlayerStats playerStats;

    private bool isActive = false;

    private Vector3 velocity;
    private Rigidbody rb;

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        rb = GetComponent<Rigidbody>();
    }

    public void Launch()
    {
        isActive = true;
        StartCoroutine(ScaleRoutine());
    }

    private void Update()
    {
        if (!isActive || rb == null) return;

        HandleHoming();
        ApplyGravityToNearby();
    }

    // CONSTANT HOMING (not coroutine-based anymore)
    private void HandleHoming()
    {
        if (target == null) return;

        Vector3 direction = (target.transform.position - transform.position).normalized;
        velocity = direction * speed;

        rb.velocity = velocity;
        transform.LookAt(target.transform);
    }

    private void ApplyGravityToNearby()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, gravityRadius, affectedLayers);

        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            // Absorb projectiles within mergeDistance
            if (distance <= mergeDistance && hit.CompareTag("Projectile"))
            {
                Destroy(hit.gameObject);
                continue;
            }

            // Pull everything else with a rigidbody
            Rigidbody otherRb = hit.attachedRigidbody;
            if (otherRb == null) continue;

            float pull = gravityStrength / Mathf.Max(distance * distance, 0.1f);
            Vector3 dir = (transform.position - hit.transform.position).normalized;
            otherRb.velocity += dir * pull * Time.deltaTime;
        }

        // Damage player
        Collider[] damageHits = Physics.OverlapSphere(transform.position, mergeDistance, damageLayers);
        foreach (Collider hit in damageHits)
        {
            Debug.Log($"Hit: {hit.gameObject.name} | Has PlayerStats: {hit.GetComponent<PlayerStats>() != null}");
            PlayerStats stats = hit.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damageAmount);
                DestroyBlackHole();
                return;
            }
        }
    }

    // PURELY VISUAL SCALING
    private IEnumerator ScaleRoutine()
    {
        float elapsed = 0f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / scaleUpDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            transform.localScale = Vector3.one * (eased * maxScale);

            yield return null;
        }

        transform.localScale = Vector3.one * maxScale;
    }

    public void DestroyBlackHole()
    {
        isActive = false;
        Destroy(gameObject);
    }
}