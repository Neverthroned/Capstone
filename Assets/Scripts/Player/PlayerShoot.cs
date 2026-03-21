using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint;         // Empty GameObject at gun barrel position
    public float fireRate = 0.1f;       // Seconds between shots

    private float nextFireTime = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}