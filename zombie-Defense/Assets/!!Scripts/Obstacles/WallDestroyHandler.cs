using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDestroyHandler : MonoBehaviour
{
    private IHealth healthComponent;

    private void Awake()
    {
        healthComponent = GetComponentInParent<IHealth>();
        if (healthComponent is Health health)
        {
            health.OnDied += HandleWallDeath;
        }
    }

    private void HandleWallDeath(GameObject obj)
    {
        var occupant = GetComponent<WallOccupant>();
        if (occupant != null)
        {
            occupant.UnoccupyCell();
        }

        if (gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                transform.localScale = Vector3.one; 
                rb.constraints &= ~(
                    RigidbodyConstraints.FreezePositionX | 
                    RigidbodyConstraints.FreezePositionY | 
                    RigidbodyConstraints.FreezePositionZ
                );
                rb.constraints &= ~(
                    RigidbodyConstraints.FreezeRotationX | 
                    RigidbodyConstraints.FreezeRotationY | 
                    RigidbodyConstraints.FreezeRotationZ
                );

                float forceAmount = 10f;
                float spinAmount = 5f;
                rb.AddForce(Vector3.up * forceAmount, ForceMode.Impulse);

                Vector3 randomTorque = new Vector3(
                    UnityEngine.Random.Range(-spinAmount, spinAmount), 
                    UnityEngine.Random.Range(-spinAmount, spinAmount), 
                    UnityEngine.Random.Range(-spinAmount, spinAmount)
                );
                rb.AddTorque(randomTorque, ForceMode.Impulse);

                StartCoroutine(DestroyWall(obj));
            }
        }
    }

    private IEnumerator DestroyWall(GameObject obj)
    {
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            children.Add(obj.transform.GetChild(i));
        }
        foreach (Transform child in children)
        {
            Destroy(child.gameObject);
        }
        NavMeshManager.Instance.RebuildNavMesh();
        CameraShake.Instance.ShakeCamera();
        yield return new WaitForSeconds(3);
        Destroy(obj);
    }
}
