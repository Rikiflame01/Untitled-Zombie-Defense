using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Enemy prefab to instantiate.")]
    public GameObject enemyPrefab;
    
    [Tooltip("Ground object that defines the spawn area. Must have a Collider and be on the 'Ground' layer.")]
    public GameObject groundObject;
    
    [Tooltip("Initial number of enemies to spawn per wave.")]
    public int initialEnemyCount = 5;
    
    [Tooltip("Multiplier applied to enemy count each wave.")]
    public float enemyMultiplier = 1.2f;
    
    [Tooltip("Duration of each wave in seconds.")]
    public float waveDuration = 20f;
    
    private int currentEnemyCount;
    private int enemiesToSpawn;
    private int waveCount = 1;
    private bool isWaveRunning = false;
    
    #region Event Subscription
    private void OnEnable()
    {
        ActionManager.OnDefenseStart += StartSpawner;
    }
    
    private void OnDisable()
    {
        ActionManager.OnDefenseStart -= StartSpawner;
    }
    #endregion

    #region Spawning Methods
    private void StartSpawner()
    {
        if (!isWaveRunning)
        {
            isWaveRunning = true;
            enemiesToSpawn = initialEnemyCount + waveCount*2;
            StartCoroutine(WaveSpawnerCoroutine());
        }
    }
    
    private IEnumerator WaveSpawnerCoroutine()
    {
        Debug.Log($"Starting Wave {waveCount} with {enemiesToSpawn} enemies.");
        currentEnemyCount = enemiesToSpawn;
        
        yield return StartCoroutine(SpawnerCoroutine(waveDuration));
                while (currentEnemyCount > 0)
        {
            yield return null;
        }
        
        waveCount++;
        enemiesToSpawn = Mathf.CeilToInt(enemiesToSpawn * enemyMultiplier);
        Debug.Log($"Wave {waveCount - 1} completed! Next wave will have {enemiesToSpawn} enemies.");
        ActionManager.InvokeDefenseStop();
        Debug.Log("Build phase starting...");
        yield return new WaitForSeconds(1f);
        ActionManager.InvokeBuildStart();
        isWaveRunning = false;
    }
    
    private IEnumerator SpawnerCoroutine(float duration)
    {
        float timeLeft = duration;
        int spawnedEnemies = 0;
        
        while (timeLeft > 0 && spawnedEnemies < enemiesToSpawn)
        {
            SpawnEnemy();
            spawnedEnemies++;
            yield return new WaitForSeconds(1f);
            timeLeft--;
        }
    }
    
    private void SpawnEnemy()
    {
        Vector3 spawnPos = GetPerimeterSpawnPosition();
        if (spawnPos == Vector3.zero)
        {
            Debug.LogWarning("No valid spawn position found on the ground perimeter.");
            return;
        }
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        
        Health enemyHealth = enemy.GetComponent<Health>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDied += OnEnemyDied;
        }
    }
    #endregion

    #region Spawn Position Calculation

    private Vector3 GetPerimeterSpawnPosition()
    {
        if (groundObject == null)
        {
            Debug.LogError("Ground object not assigned.");
            return Vector3.zero;
        }
        
        Collider groundCollider = groundObject.GetComponent<Collider>();
        if (groundCollider == null)
        {
            Debug.LogError("Ground object does not have a Collider.");
            return Vector3.zero;
        }
        
        Bounds bounds = groundCollider.bounds;
        Vector3 spawnPos = Vector3.zero;
        
        int edge = Random.Range(0, 4);
        switch (edge)
        {
            case 0: // Bottom edge (min.z)
                spawnPos = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y, bounds.min.z);
                break;
            case 1: // Top edge (max.z)
                spawnPos = new Vector3(Random.Range(bounds.min.x, bounds.max.x), bounds.center.y, bounds.max.z);
                break;
            case 2: // Left edge (min.x)
                spawnPos = new Vector3(bounds.min.x, bounds.center.y, Random.Range(bounds.min.z, bounds.max.z));
                break;
            case 3: // Right edge (max.x)
                spawnPos = new Vector3(bounds.max.x, bounds.center.y, Random.Range(bounds.min.z, bounds.max.z));
                break;
        }
        
        RaycastHit hit;
        Vector3 rayOrigin = new Vector3(spawnPos.x, bounds.max.y + 1f, spawnPos.z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, bounds.size.y + 2f))
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                spawnPos.y = hit.point.y;
                return spawnPos;
            }
            else
            {
                Debug.LogWarning("Raycast did not hit an object on the Ground layer.");
                return Vector3.zero;
            }
        }
        return Vector3.zero;
    }
    #endregion

    #region Enemy Death Handling

    private void OnEnemyDied(GameObject enemy)
    {
        currentEnemyCount--;
        Destroy(enemy);
    }
    #endregion
}
