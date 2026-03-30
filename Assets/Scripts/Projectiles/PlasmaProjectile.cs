using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaProjectile : MonoBehaviour
{
    public float scaleUpDuration = 0.5f;
    public float maxScale = 1.0f;
    public float bounceDamping = 0.85f;
    public float acceleration = 5f;        // How quickly it reaches max speed
    public float maxSpeed = 15f;           // Speed cap
    public int maxBounces = 5;             // Bounces before destruction

    private PlayerStats playerStats;

    private Vector3 velocity;
    private int bounceCount = 0;

    [Header("Damage")]
    public float damageAmount = 20f;
    [SerializeField] private LayerMask damageLayers;  // Layers that the white hole can damage
    [SerializeField] private float damageRadius = 3f; // AoE radius

    private Rigidbody rb;

    private bool justBounced = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void Launch(Vector3 initialVelocity)
    {
        velocity = initialVelocity.magnitude > 0.1f
            ? initialVelocity.normalized * Mathf.Max(initialVelocity.magnitude, maxSpeed * 0.3f)
            : Random.onUnitSphere * maxSpeed * 0.3f;
        rb.velocity = velocity;
        StartCoroutine(ScaleRoutine());
    }

    private void Update()
    {
        if (BlackHoleProjectile.ActiveBlackHoles.Count > 0)
        {
            var blackHole = BlackHoleProjectile.ActiveBlackHoles[0];
            if (blackHole != null)
            {
                Vector3 direction = (blackHole.transform.position - transform.position).normalized;
                rb.velocity = Vector3.Lerp(rb.velocity, direction * maxSpeed, Time.deltaTime * 5f);
            }
            return; // Skip normal acceleration entirely
        }

        // Normal acceleration when no black hole
        if (rb.velocity.magnitude < maxSpeed)
            rb.velocity += rb.velocity.normalized * acceleration * Time.deltaTime;
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        int otherLayer = collision.gameObject.layer;

        if ((damageLayers.value & (1 << otherLayer)) != 0)
        {
            PlayerStats stats = collision.gameObject.GetComponentInParent<PlayerStats>();
            if (stats != null) stats.TakeDamage(damageAmount);
            DestroyPlasma();
            return;
        }

        Vector3 normal = collision.contacts[0].normal;
        // Ensure bouncing happens
        float currentSpeed = Mathf.Max(rb.velocity.magnitude, maxSpeed * 0.5f);
        rb.velocity = Vector3.Reflect(rb.velocity.normalized, normal) * currentSpeed * bounceDamping;
        justBounced = true;

        bounceCount++;
        if (bounceCount >= maxBounces) DestroyPlasma();
    }

    private IEnumerator ScaleRoutine()
    {
        float elapsed = 0f;

        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);
            transform.localScale = Vector3.one * (easedT * maxScale);

            yield return null;
        }

        transform.localScale = Vector3.one * maxScale;
    }

    public void DestroyPlasma()
    {
        Destroy(gameObject);
    }
}