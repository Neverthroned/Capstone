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

    [Header("Merge")]
    public GameObject whiteHolePrefab;

    [Header("Damage")]
    public float damageAmount = 10f;

    [SerializeField] private LayerMask affectedLayers;
    [SerializeField] private LayerMask damageLayers;

    private PlayerStats playerStats;

    private bool isActive = false;
    private bool hasMerged = false;

    private Vector3 velocity;
    private Rigidbody rb;

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        rb = GetComponent<Rigidbody>();

        // Safety
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
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

            Rigidbody otherRb = hit.attachedRigidbody;
            if (otherRb == null) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            float pull = gravityStrength / Mathf.Max(distance * distance, 0.1f);

            Vector3 dir = (transform.position - hit.transform.position).normalized;

            otherRb.velocity += dir * pull * Time.deltaTime;
        }

        // Damage player
        Collider[] damageHits = Physics.OverlapSphere(transform.position, 0.5f, damageLayers);
        foreach (Collider hit in damageHits)
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(damageAmount);
                DestroyBlackHole();
                return;
            }
        }
    }

    // MERGE VIA COLLISION (reliable now)
    private void OnCollisionEnter(Collision collision)
    {
        BlackHoleProjectile other = collision.gameObject.GetComponent<BlackHoleProjectile>();

        if (other != null && !hasMerged && !other.hasMerged)
        {
            hasMerged = true;
            other.hasMerged = true;

            Vector3 midpoint = (transform.position + other.transform.position) / 2f;
            Vector3 combinedVelocity = rb.velocity + other.rb.velocity;

            GameObject whiteHole = Instantiate(whiteHolePrefab, midpoint, Quaternion.identity);

            WhiteHole wh = whiteHole.GetComponent<WhiteHole>();
            wh.scaleUpDuration = scaleUpDuration;
            wh.maxScale = maxScale;
            wh.Launch(combinedVelocity);

            other.DestroyBlackHole();
            DestroyBlackHole();
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