using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class PooledBullet : MonoBehaviour
{
    public EntityStats playerStats;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    private Rigidbody rb;
    
    [SerializeField] private GameObject bloodBurstPrefab;
    public GameObject bulletHitVFX;

    private bool hitTarget = false;
    private Vector3 lastVelocity;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DespawnBullet());
    }
    
    private void FixedUpdate()
    {
        if(rb != null)
            lastVelocity = rb.linearVelocity;
    }

    private IEnumerator DespawnBullet()
    {
        yield return new WaitForSeconds(3f);
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(bulletHitVFX, transform.position, transform.rotation);
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            hitTarget = true;
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(playerStats.damage);
                SoundManager.Instance.PlaySFX("bulletHit", 0.7f);

                Vector3 knockbackDirection = lastVelocity.normalized;
                
                BaseMeleeEnemy enemyAI = collision.gameObject.GetComponent<BaseMeleeEnemy>();
                if (enemyAI != null)
                {
                    enemyAI.StartCoroutine(enemyAI.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration));
                }
                else
                {
                    Rigidbody enemyRb = collision.gameObject.GetComponent<Rigidbody>();
                    if (enemyRb != null)
                    {
                        enemyRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
                    }
                }
                
                if (health.currentHealth <= 0)
                {
                    ContactPoint contact = collision.contacts[0];
                    Vector3 spawnPosition = contact.point;
                    
                    GameObject playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                    {
                        Vector3 directionAwayFromPlayer = (spawnPosition - playerObj.transform.position).normalized;
                        Quaternion spawnRotation = Quaternion.LookRotation(directionAwayFromPlayer);
                        Instantiate(bloodBurstPrefab, spawnPosition, spawnRotation);
                    }
                    else
                    {
                        Instantiate(bloodBurstPrefab, spawnPosition, Quaternion.identity);
                    }
                }
            }
            ReturnToPool();
        }
        else if (!hitTarget && gameObject.activeSelf)
        {
            StartCoroutine(DespawnBullet());
        }
    }

    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
}
