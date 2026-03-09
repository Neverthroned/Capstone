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

    private Vector3 velocity;
    private int bounceCount = 0;

    public void Launch(Vector3 initialVelocity)
    {
        // Use initial direction but enforce a minimum launch speed so slow black holes still produce a lively white hole
        velocity = initialVelocity.magnitude > 0.1f
            ? initialVelocity.normalized * Mathf.Max(initialVelocity.magnitude, maxSpeed * 0.3f)
            : Random.onUnitSphere * maxSpeed * 0.3f;    // Fallback if black holes somehow had no velocity

        StartCoroutine(ScaleUp());
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

    private IEnumerator ScaleUp()
    {
        transform.localScale = Vector3.zero;
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
}