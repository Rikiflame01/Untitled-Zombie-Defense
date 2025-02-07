using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using Unity.AI.Navigation;

public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager Instance { get; private set; }
    private NavMeshSurface navMeshSurface;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        navMeshSurface = FindFirstObjectByType<NavMeshSurface>();

        if (navMeshSurface == null)
        {
            Debug.LogError("No NavMeshSurface found in the scene! Please add one.");
        }
    }

    public void RebuildNavMesh()
    {
        if (navMeshSurface != null)
        {
            StartCoroutine(RebuildNavMeshCoroutine());
        }
        else
        {
            Debug.LogError("NavMeshSurface is missing! Cannot rebuild NavMesh.");
        }
    }

    private IEnumerator RebuildNavMeshCoroutine()
    {
        yield return null;
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh rebuilt successfully!");
    }
}
