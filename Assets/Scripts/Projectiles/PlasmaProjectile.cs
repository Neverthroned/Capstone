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
    private float _lastBounceTime = -1f;
    private float _bounceCooldown = 0.1f;


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
        rb.isKinematic = true; // Switch to kinematic, we move it manually
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        playerStats = FindFirstObjectByType<PlayerStats>();
    }

    public void Launch(Vector3 initialVelocity)
    {
        velocity = initialVelocity.magnitude > 0.1f
            ? initialVelocity.normalized * Mathf.Max(initialVelocity.magnitude, maxSpeed * 0.3f)
            : Random.onUnitSphere * maxSpeed * 0.3f;
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
                velocity = Vector3.Lerp(velocity, direction * maxSpeed, Time.deltaTime * 5f);
            }
        }
        else
        {
            if (velocity.magnitude < maxSpeed)
                velocity += velocity.normalized * acceleration * Time.deltaTime;
            velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        }

        // Raycast ahead to detect walls before moving through them
        float moveDistance = velocity.magnitude * Time.deltaTime;
        if (Physics.Raycast(transform.position, velocity.normalized, out RaycastHit hit, moveDistance + 0.1f))
        {
            int hitLayer = hit.collider.gameObject.layer;

            // Check if it's the player layer
            if ((damageLayers.value & (1 << hitLayer)) != 0)
            {
                PlayerStats stats = hit.collider.GetComponentInParent<PlayerStats>();
                if (stats != null) stats.TakeDamage(damageAmount);
                DestroyPlasma();
                return;
            }

            // Otherwise bounce
            if (Time.time - _lastBounceTime >= _bounceCooldown)
            {
                _lastBounceTime = Time.time;
                float currentSpeed = Mathf.Max(velocity.magnitude, maxSpeed * 0.5f);
                velocity = Vector3.Reflect(velocity.normalized, hit.normal) * currentSpeed * bounceDamping;
                transform.position += hit.normal * 0.1f;
                bounceCount++;
                if (bounceCount >= maxBounces)
                {
                    DestroyPlasma();
                    return;
                }
            }
        }

        rb.MovePosition(transform.position + velocity * Time.deltaTime);
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