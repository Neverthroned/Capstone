using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class BlackHole : MonoBehaviour
{
    public GameObject BlackHolePrefab;
    public List<GameObject> spawnPositions;
    public GameObject target;
    public float speed = 1.0f;
    public float scaleUpDuration = 0.5f;
    public float maxScale = 1.0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameObject blackHole = Instantiate(BlackHolePrefab, spawnPositions[Random.Range(0, spawnPositions.Count)].transform.position, BlackHolePrefab.transform.rotation);
            blackHole.transform.localScale = Vector3.zero;
            blackHole.transform.LookAt(target.transform);

            BlackHoleProjectile projectile = blackHole.GetComponent<BlackHoleProjectile>();
            projectile.target = target;
            projectile.speed = speed;
            projectile.scaleUpDuration = scaleUpDuration;
            projectile.maxScale = maxScale;
            projectile.Launch();
        }
    }
}