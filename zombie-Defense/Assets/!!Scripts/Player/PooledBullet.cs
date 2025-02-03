using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PooledBullet : MonoBehaviour
{
    private Rigidbody rb;

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
        StartCoroutine(DespawnBullet());
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            Health health = collision.gameObject.GetComponent<Health>();
            
            if (health != null)
            {
                health.TakeDamage(10);
            }

            ReturnToPool();
        }
    }


    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        Debug.Log("Returning bullet to pool");
        ObjectPooler.Instance.ReturnToPool(gameObject);
    }
}
