using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float damageAmount = 1.5f;
    public float lifetime = 5f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    // Remove Update entirely - Rigidbody handles movement now

    private void OnTriggerEnter(Collider other)
    {
        // Ignore the player
        if (other.CompareTag("Player")) return;

        Debug.Log($"Projectile hit: {other.gameObject.name}");

        BossStats boss = other.GetComponentInParent<BossStats>();
        if (boss != null)
        {
            boss.TakeDamage(damageAmount);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}