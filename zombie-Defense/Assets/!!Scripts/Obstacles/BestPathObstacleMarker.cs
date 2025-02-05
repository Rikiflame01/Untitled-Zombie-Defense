using UnityEngine;

public class BestPathObstacleMarker : MonoBehaviour
{
    [Tooltip("Transform to shoot the raycasts from. If null, defaults to this GameObject's transform.")]
    public Transform raycastOrigin;

    [Tooltip("How far the raycasts should go.")]
    public float raycastDistance = 100f;
    
    [Tooltip("Layer mask used to detect obstacles.")]
    public LayerMask obstacleLayer;

    [Tooltip("Material to assign to obstacles for debugging.")]
    public Material debugBlueMaterial;

    private void OnEnable()
    {
        ActionManager.OnDefenseStart += MarkBestPathObstacles;
    }

    private void OnDisable()
    {
        ActionManager.OnDefenseStart -= MarkBestPathObstacles;
    }

    private void MarkBestPathObstacles()
    {
        BestPathManager.ClearBestPaths(); // old best paths

        Vector3 originPos = (raycastOrigin != null) ? raycastOrigin.position : transform.position;

        Vector3[] directions = new Vector3[]
        {
            new Vector3(0, 0, 1),   // Global North
            new Vector3(0, 0, -1),  // Global South
            new Vector3(1, 0, 0),   // Global East
            new Vector3(-1, 0, 0)   // Global West
        };

        foreach (Vector3 dir in directions)
        {
            RaycastHit[] hits = Physics.RaycastAll(originPos, dir, raycastDistance, obstacleLayer);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit hit in hits)
            {
                BestPathManager.AddBestPathObstacle(hit.transform);

                Renderer rend = hit.transform.GetComponent<Renderer>();
                if (rend != null && debugBlueMaterial != null)
                {
                    rend.material = debugBlueMaterial;
                }
                
            }
        }
    }
}
