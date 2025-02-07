using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PooledBullet : MonoBehaviour
{
    private Rigidbody rb;

    private bool hitTarget = false;
    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
    }

    private IEnumerator DespawnBullet()
    {
        yield return new WaitForSeconds(3);
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
            }
            hitTarget = false;
            StopAllCoroutines();
            ReturnToPool();
        }
        else{
    if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
    {
        hitTarget = true;
        Health health = collision.gameObject.GetComponent<Health>();
        
        if (health != null)
        {
            health.TakeDamage(34);
        }
        hitTarget = false;
        StopAllCoroutines();
        ReturnToPool();
    }
            else if (hitTarget == false && this.gameObject.activeSelf)
            {
                StartCoroutine(DespawnBullet());
            }
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
