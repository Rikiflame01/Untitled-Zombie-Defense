using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PoolItem
{
    public string tag;
    public GameObject prefab;
    public int size;
}

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    [Header("Pool Items")]
    public List<PoolItem> poolItems;

    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (PoolItem item in poolItems)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                obj.SetActive(false);

                Poolable poolable = obj.GetComponent<Poolable>();
                if (poolable == null) poolable = obj.AddComponent<Poolable>();
                poolable.poolTag = item.tag;

                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(item.tag, objectPool);
        }
    }

    public GameObject GetPooledObject(string tag)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("No object available in pool " + tag);
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public GameObject GetPooledObject(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            return null;
        }

        if (poolDictionary[tag].Count == 0)
        {
            Debug.LogWarning("No object available in pool " + tag);
            return null;
        }

        GameObject obj = poolDictionary[tag].Dequeue();

        obj.transform.SetParent(null);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        obj.SetActive(true);

        return obj;
    }
    public void ReturnToPool(GameObject obj)
    {
        Poolable poolable = obj.GetComponent<Poolable>();
        if (poolable == null)
        {
            Debug.LogWarning("Returned object lacks Poolable component. Destroying.");
            Destroy(obj);
            return;
        }

        string tag = poolable.poolTag;
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
            Destroy(obj);
            return;
        }

        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero; 
        obj.transform.localRotation = Quaternion.identity;

        obj.SetActive(false);
        poolDictionary[tag].Enqueue(obj);
    }
}
