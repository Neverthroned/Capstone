using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlasmaProjectile : MonoBehaviour
{
    public float maxSpeed = 15f;
    public float bounceDamping = 0.85f;
    public int maxBounces = 5;
    public float scaleUpDuration = 0.5f;
    public float maxScale = 1.0f;

    [Header("Damage")]
    public float damageAmount = 20f;

    private Rigidbody rb;
    private int bounceCount = 0;
    private float _lastBounceTime = -1f;
    private float _bounceCooldown = 0.1f;
    private Vector3 _lastVelocity;


    [SerializeField] private LayerMask wallLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    public void Launch(Vector3 initialVelocity)
    {
        rb.velocity = initialVelocity.normalized * maxSpeed;
        StartCoroutine(ScaleRoutine());
    }

    private void Update()
    {
        // Store velocity before physics modifies it
        _lastVelocity = rb.velocity;

        if (BlackHoleProjectile.ActiveBlackHoles.Count > 0)
        {
            var blackHole = BlackHoleProjectile.ActiveBlackHoles[0];
            if (blackHole != null)
            {
                Vector3 direction = (blackHole.transform.position - transform.position).normalized;
                rb.velocity = Vector3.Lerp(rb.velocity, direction * maxSpeed, Time.deltaTime * 5f);
            }
        }

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - _lastBounceTime < _bounceCooldown) return;
        if ((wallLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        _lastBounceTime = Time.time;

        Vector3 normal = collision.contacts[0].normal;

        // Reflect the pre-collision velocity at full maxSpeed, ignoring what physics did to it
        Vector3 reflected = Vector3.Reflect(_lastVelocity.normalized, normal) * maxSpeed;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.velocity = reflected;

        transform.position += normal * 0.05f;

        bounceCount++;
        if (bounceCount >= maxBounces) DestroyPlasma();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Hit player
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeDamage(damageAmount);
            DestroyPlasma();
            return;
        }

        // Hit black hole
        BlackHoleProjectile blackHole = other.GetComponentInParent<BlackHoleProjectile>();
        if (blackHole != null)
        {
            DestroyPlasma();
            return;
        }
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