using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PooledBullet : MonoBehaviour
{
    private Rigidbody rb;
    
    [SerializeField] private GameObject bloodBurstPrefab;
    
    private bool hitTarget = false;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(DespawnBullet());
    }

    private IEnumerator DespawnBullet()
    {
        yield return new WaitForSeconds(3f);
        ReturnToPool();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            hitTarget = true;
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage(34);
                SoundManager.Instance.PlaySFX("bulletHit", 0.7f);

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
            StopAllCoroutines();
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
