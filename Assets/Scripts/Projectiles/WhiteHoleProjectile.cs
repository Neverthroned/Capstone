using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteHoleProjectile : MonoBehaviour
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

    private void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false; // Must be false for OnCollisionEnter to fire
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
        // Accelerate up to max speed
        if (rb.velocity.magnitude < maxSpeed)
            rb.velocity += rb.velocity.normalized * acceleration * Time.deltaTime;

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {

        int otherLayer = collision.gameObject.layer;

        // DAMAGE PLAYER (layer-based)
        if ((damageLayers.value & (1 << otherLayer)) != 0)
        {
            Debug.Log($"playerStats is null: {playerStats == null}");
            PlayerStats stats = collision.gameObject.GetComponentInParent<PlayerStats>();
            if (stats != null)
            {
                stats.TakeDamage(damageAmount);
            }

            DestroyWhiteHole();
            return;
        }

        // OTHERWISE: bounce
        Vector3 normal = collision.contacts[0].normal;
        velocity = Vector3.Reflect(velocity, normal) * bounceDamping;

        rb.velocity = velocity;

        bounceCount++;

        if (bounceCount >= maxBounces)
            DestroyWhiteHole();
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

    public void DestroyWhiteHole()
    {
        Destroy(gameObject);
    }
}