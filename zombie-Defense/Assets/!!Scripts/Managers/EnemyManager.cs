using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Pool Settings")]
    [Tooltip("The pool tag corresponding to enemy objects.")]
    public string enemyPoolTag = "Enemy";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy();
        }
    }

    public void SpawnEnemy()
    {
        GameObject enemy = ObjectPooler.Instance.GetPooledObject(enemyPoolTag);
        if (enemy != null)
        {
            Vector3 spawnPos = GetRandomNavMeshPerimeterPosition();
            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;

            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.OnDied += OnEnemyDied;
            }
        }
        else
        {
            Debug.Log("No enemy available in the pool to spawn.");
        }
    }

    private Vector3 GetRandomNavMeshPerimeterPosition()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        if (navMeshData.vertices.Length == 0)
        {
            Debug.LogWarning("NavMesh triangulation data is empty.");
            return Vector3.zero;
        }

        Vector3 min = navMeshData.vertices[0];
        Vector3 max = navMeshData.vertices[0];
        foreach (Vector3 vertex in navMeshData.vertices)
        {
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }

        int edge = UnityEngine.Random.Range(0, 4);
        Vector3 randomPoint = Vector3.zero;
        switch (edge)
        {
            case 0:
                randomPoint = new Vector3(UnityEngine.Random.Range(min.x, max.x), 0, min.z);
                break;
            case 1:
                randomPoint = new Vector3(UnityEngine.Random.Range(min.x, max.x), 0, max.z);
                break;
            case 2:
                randomPoint = new Vector3(min.x, 0, UnityEngine.Random.Range(min.z, max.z));
                break;
            case 3:
                randomPoint = new Vector3(max.x, 0, UnityEngine.Random.Range(min.z, max.z));
                break;
        }

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        else
        {
            Debug.LogWarning("Failed to sample a valid navmesh position near " + randomPoint);
            return randomPoint;
        }
    }

    private void OnEnemyDied(GameObject enemy)
    {
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= OnEnemyDied;
        }
        ObjectPooler.Instance.ReturnToPool(enemy);
    }
}
