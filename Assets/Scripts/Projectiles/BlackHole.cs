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
        // Placeholder Spawn
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameObject BlackHole = Instantiate(BlackHolePrefab, spawnPositions[Random.Range(0, 1)].transform.position, BlackHolePrefab.transform.rotation);
            BlackHole.transform.localScale = Vector3.zero;
            BlackHole.transform.LookAt(target.transform);
            StartCoroutine(SendHoming(BlackHole));
        }
    }

    public IEnumerator SendHoming(GameObject BlackHole)
    {
        // Scale up phase
        float elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleUpDuration);
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Ease-out cubic
            BlackHole.transform.localScale = Vector3.one * (easedT * maxScale);

            BlackHole.transform.position += (target.transform.position - BlackHole.transform.position).normalized * speed * Time.deltaTime;
            BlackHole.transform.LookAt(target.transform);
            yield return null;
        }

        BlackHole.transform.localScale = Vector3.one * maxScale;

        // Homing phase
        while (Vector3.Distance(target.transform.position, BlackHole.transform.position) > 0.3f)
        {
            BlackHole.transform.position += (target.transform.position - BlackHole.transform.position).normalized * speed * Time.deltaTime;
            BlackHole.transform.LookAt(target.transform);
            yield return null;
        }

        Destroy(BlackHole);
    }
}