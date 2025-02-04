using System;
using System.Collections;
using UnityEngine;

public class WallDestroyHandler : MonoBehaviour
{
    private IHealth healthComponent;

    void Awake()
    {
        healthComponent = GetComponentInParent<IHealth>();

        if (healthComponent is Health health)
        {
            health.OnDied += HandleWallDeath;
        }
    }

private void HandleWallDeath(GameObject @object)
{
    if (gameObject.layer == LayerMask.NameToLayer("Obstacle"))
    {
        Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

        if (rigidbody != null)
        {
            transform.localScale = Vector3.one; 
            
            rigidbody.constraints &= ~(RigidbodyConstraints.FreezePositionX 
                                    | RigidbodyConstraints.FreezePositionY 
                                    | RigidbodyConstraints.FreezePositionZ);
            rigidbody.constraints &= ~(RigidbodyConstraints.FreezeRotationX
                                    | RigidbodyConstraints.FreezeRotationY
                                    | RigidbodyConstraints.FreezeRotationZ);

            float forceAmount = 10f;
            float spinAmount = 5f;

            rigidbody.AddForce(Vector3.up * forceAmount, ForceMode.Impulse);

            Vector3 randomTorque = new Vector3(
                UnityEngine.Random.Range(-spinAmount, spinAmount), 
                UnityEngine.Random.Range(-spinAmount, spinAmount), 
                UnityEngine.Random.Range(-spinAmount, spinAmount)
            );

            rigidbody.AddTorque(randomTorque, ForceMode.Impulse);
            StartCoroutine(DestroyWall(@object));
        }
    }
}

    private IEnumerator DestroyWall(GameObject gameObject)
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
