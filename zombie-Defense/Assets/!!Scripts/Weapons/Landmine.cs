using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Landmine : MonoBehaviour
{
    [Header("Explosion Settings")]
    [Tooltip("Radius within which enemies will be affected.")]
    public float explosionRadius = 5f;
    [Tooltip("The base explosion force applied to enemies.")]
    public float explosionForce = 700f;
    [Tooltip("Additional upward force modifier.")]
    public float upwardModifier = 2f;
    [Tooltip("Extra multiplier range to add random variation to the force.")]
    public float randomForceMin = 0.8f;
    public float randomForceMax = 1.2f;

    [Header("Damage Settings")]
    [Tooltip("Damage applied to each enemy in range.")]
    public int damageAmount = 100;
    [Tooltip("Layer mask for enemy objects (assign the 'Enemy' layer here).")]
    public LayerMask enemyLayer;

    [Header("Death VFX Settings")]
    [Tooltip("First visual effect prefab to spawn on death.")]
    public GameObject deathVFX1;
    [Tooltip("Delay before spawning the first VFX (in seconds).")]
    public float deathVFX1Delay = 0f;
    [Tooltip("Second visual effect prefab to spawn on death.")]
    public GameObject deathVFX2;
    [Tooltip("Delay before spawning the second VFX (in seconds).")]
    public float deathVFX2Delay = 0f;

    private bool hasExploded = false;
    private void OnTriggerEnter(Collider other)
    {
        if (!hasExploded && other.gameObject.CompareTag("Enemy"))
        {
            SoundManager.Instance.PlaySFX("explosion", 1f);
            hasExploded = true;
            Explode();
        }
    }

    private void Explode()
    {
        LandmineOccupant landmineOccupant = GetComponent<LandmineOccupant>();
        landmineOccupant.UnoccupyCell();
        List<Vector3> enemyPositions = new List<Vector3>();

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, enemyLayer);
        foreach (Collider hit in colliders)
        {
            Health enemyHealth = hit.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
            }

            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float randomMultiplier = Random.Range(randomForceMin, randomForceMax);
                rb.AddExplosionForce(explosionForce * randomMultiplier, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);

                Vector3 randomUpward = new Vector3(Random.Range(-1f, 1f), 1f, Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomUpward * explosionForce * 0.2f, ForceMode.Impulse);
            }

            enemyPositions.Add(hit.transform.position);
        }

        if (deathVFX1 != null)
        {
            Invoke("SpawnDeathVFX1", deathVFX1Delay);
        }

        if (deathVFX2 != null)
        {
            StartCoroutine(SpawnDeathVFX2Coroutine(enemyPositions));
        }
        else
        {
            float destroyDelay = deathVFX1Delay + 0.1f;
            Destroy(gameObject, destroyDelay);
        }
    }

    private void SpawnDeathVFX1()
    {
        Instantiate(deathVFX1, transform.position, transform.rotation);
    }

    private IEnumerator SpawnDeathVFX2Coroutine(List<Vector3> enemyPositions)
    {
        yield return new WaitForSeconds(deathVFX2Delay);

        foreach (Vector3 pos in enemyPositions)
        {
            Instantiate(deathVFX2, pos, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
