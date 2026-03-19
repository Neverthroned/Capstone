using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteHole : MonoBehaviour
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


    public void Start()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void Launch(Vector3 initialVelocity)
    {
        // Use initial direction but enforce a minimum launch speed so slow black holes still produce a lively white hole
        velocity = initialVelocity.magnitude > 0.1f
            ? initialVelocity.normalized * Mathf.Max(initialVelocity.magnitude, maxSpeed * 0.3f)
            : Random.onUnitSphere * maxSpeed * 0.3f;    // Fallback if black holes somehow had no velocity

        StartCoroutine(ScaleAndDamageRoutine());
    }

    private void Update()
    {
        if (velocity.magnitude < maxSpeed)
            velocity += velocity.normalized * acceleration * Time.deltaTime;

        // Hard clamp so it never exceeds max
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        velocity = Vector3.Reflect(velocity, collision.contacts[0].normal) * bounceDamping;
        bounceCount++;

        if (bounceCount >= maxBounces)
            Destroy(gameObject);
    }

    private IEnumerator ScaleAndDamageRoutine()
    {
        float elapsed = 0f;

        // Scale up animation
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f);  // cubic ease out
            transform.localScale = Vector3.one * (easedT * maxScale);

            // Apply damage each frame
            ApplyDamage();

            yield return null;
        }

        transform.localScale = Vector3.one * maxScale;

        // Optional: keep damaging objects for a short duration
        float sustainTime = 1f;
        elapsed = 0f;
        while (elapsed < sustainTime)
        {
            elapsed += Time.deltaTime;
            ApplyDamage();
            yield return null;
        }
    }

    private void ApplyDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, damageLayers);

        foreach (Collider hit in hits)
        {
            if (playerStats != null)
            {
                playerStats.TakeDamage(damageAmount);
                DestroyWhiteHole();
                return;
            }
        }
    }
    public void DestroyWhiteHole()
    {
        Destroy(gameObject);
    }
}