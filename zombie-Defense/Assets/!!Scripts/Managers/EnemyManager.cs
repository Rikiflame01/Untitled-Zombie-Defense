using UnityEngine;
using UnityEngine.AI;

public class EnemyManager : MonoBehaviour
{
    [Header("Enemy Pool Settings")]
    [Tooltip("The pool tag corresponding to enemy objects.")]
    public string enemyPoolTag = "Enemy";

    private void Update()
    {
        // For demonstration, press Space to spawn an enemy.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnEnemy();
        }
    }

    /// <summary>
    /// Retrieves an enemy from the pool and spawns it at a random perimeter point on the navmesh.
    /// Also subscribes to the enemy's OnDied event.
    /// </summary>
    public void SpawnEnemy()
    {
        GameObject enemy = ObjectPooler.Instance.GetPooledObject(enemyPoolTag);
        if (enemy != null)
        {
            Vector3 spawnPos = GetRandomNavMeshPerimeterPosition();
            enemy.transform.position = spawnPos;
            enemy.transform.rotation = Quaternion.identity;

            // Subscribe to the enemy's OnDied event (from the Health component).
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

    /// <summary>
    /// Calculates a random spawn position along the perimeter of the navmesh.
    /// </summary>
    private Vector3 GetRandomNavMeshPerimeterPosition()
    {
        // Retrieve triangulation data from the navmesh.
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();
        if (navMeshData.vertices.Length == 0)
        {
            Debug.LogWarning("NavMesh triangulation data is empty.");
            return Vector3.zero;
        }

        // Determine the bounding rectangle for the navmesh using its vertices.
        Vector3 min = navMeshData.vertices[0];
        Vector3 max = navMeshData.vertices[0];
        foreach (Vector3 vertex in navMeshData.vertices)
        {
            min = Vector3.Min(min, vertex);
            max = Vector3.Max(max, vertex);
        }

        // Randomly select one of the four edges of the bounding rectangle.
        int edge = UnityEngine.Random.Range(0, 4);
        Vector3 randomPoint = Vector3.zero;
        switch (edge)
        {
            case 0: // Bottom edge
                randomPoint = new Vector3(UnityEngine.Random.Range(min.x, max.x), 0, min.z);
                break;
            case 1: // Top edge
                randomPoint = new Vector3(UnityEngine.Random.Range(min.x, max.x), 0, max.z);
                break;
            case 2: // Left edge
                randomPoint = new Vector3(min.x, 0, UnityEngine.Random.Range(min.z, max.z));
                break;
            case 3: // Right edge
                randomPoint = new Vector3(max.x, 0, UnityEngine.Random.Range(min.z, max.z));
                break;
        }

        // Use NavMesh.SamplePosition to ensure the random point is on the navmesh.
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

    /// <summary>
    /// Callback for when an enemy dies. Unsubscribes the event and returns the enemy to the pool.
    /// </summary>
    private void OnEnemyDied(GameObject enemy)
    {
        // Unsubscribe from the OnDied event to avoid duplicate calls.
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDied -= OnEnemyDied;
        }
        // Return the enemy to the pool.
        ObjectPooler.Instance.ReturnToPool(enemy);
    }
}
