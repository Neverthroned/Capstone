using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float damageAmount = 1.5f;
    public float lifetime = 5f;
    public float blackHoleInfluence = 2f; // Tune how strongly black hole pulls player bullets

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void Start()
    {
        rb.velocity = transform.forward * speed;
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (BlackHoleProjectile.ActiveBlackHoles.Count > 0)
        {
            var blackHole = BlackHoleProjectile.ActiveBlackHoles[0];
            if (blackHole != null)
            {
                Vector3 direction = (blackHole.transform.position - transform.position).normalized;
                rb.velocity = Vector3.Lerp(rb.velocity, direction * speed, Time.deltaTime * blackHoleInfluence);
            }
            return;
        }
    }

    // Collision
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
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