using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plasma : MonoBehaviour
{
    public GameObject PlasmaPrefab;
    public List<GameObject> spawnPositions;
    public GameObject target;
    public float speed = 1.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnPlasma();
        }
    }

    public void SpawnPlasma()
    {
        Transform spawnPoint = spawnPositions[Random.Range(0, spawnPositions.Count)].transform;

        GameObject plasma = Instantiate(PlasmaPrefab, spawnPoint.position, PlasmaPrefab.transform.rotation);
        plasma.transform.localScale = Vector3.zero;

        PlasmaProjectile projectile = plasma.GetComponent<PlasmaProjectile>();

        if (projectile == null)
        {
            Debug.LogError("PlasmaPrefab is missing a PlasmaProjectile component!");
            return;
        }

        // Direction from spawn point toward the player
        Vector3 direction = (target.transform.position - spawnPoint.position).normalized;
        projectile.Launch(direction * speed);
    }
}