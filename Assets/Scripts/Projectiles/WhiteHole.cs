using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteHole : MonoBehaviour
{
    public GameObject WhiteHolePrefab;
    public List<GameObject> spawnPositions;
    public GameObject target;
    public float speed = 1.0f;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject blackHole = Instantiate(WhiteHolePrefab, spawnPositions[Random.Range(0, spawnPositions.Count)].transform.position, WhiteHolePrefab.transform.rotation);
            blackHole.transform.localScale = Vector3.zero;
            blackHole.transform.LookAt(target.transform);

            BlackHoleProjectile projectile = blackHole.GetComponent<BlackHoleProjectile>();
            projectile.target = target;
            projectile.speed = speed;
            projectile.Launch();
        }
    }
}
