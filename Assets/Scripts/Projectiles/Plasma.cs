using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plasma : MonoBehaviour
{
    public GameObject PlasmaPrefab;
    public List<GameObject> spawnPositions;
    public GameObject target;
    public float speed = 1.0f;
    public float spreadAngle = 45f; // Total cone spread in degrees

    public void SpawnPlasma(int count)
    {
        AudioManager.Instance.PlayPlasmaShoot();

        Transform spawnPoint = spawnPositions[Random.Range(0, spawnPositions.Count)].transform;
        Vector3 baseDirection = (target.transform.position - spawnPoint.position).normalized;

        for (int i = 0; i < count; i++)
        {
            // Spread projectiles evenly across the cone
            float angle = count > 1
                ? Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, i / (float)(count - 1))
                : 0f;

            // Rotate the direction by the spread angle on the Y axis
            Vector3 spreadDirection = Quaternion.Euler(0, angle, 0) * baseDirection;

            GameObject plasma = Instantiate(PlasmaPrefab, spawnPoint.position, PlasmaPrefab.transform.rotation);
            plasma.transform.localScale = Vector3.zero;

            PlasmaProjectile projectile = plasma.GetComponent<PlasmaProjectile>();
            if (projectile == null)
            {
                Debug.LogError("PlasmaPrefab is missing PlasmaProjectile component!");
                continue;
            }

            projectile.Launch(spreadDirection * speed);
        }
    }
}