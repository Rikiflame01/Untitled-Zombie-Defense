using UnityEngine;
using System.Collections.Generic;

public static class BestPathManager
{
    // List of currently marked obstacles.
    private static List<Transform> bestPathObstacles = new List<Transform>();

    // Store each obstacle’s original material.
    private static Dictionary<Transform, Material> originalMaterials = new Dictionary<Transform, Material>();

    /// Clears the list of best path obstacles and reverts their materials.
    public static void ClearBestPaths()
    {
        foreach (Transform obstacle in bestPathObstacles)
        {
            if (obstacle != null)
            {
                Renderer rend = obstacle.GetComponent<Renderer>();
                if (rend != null && originalMaterials.ContainsKey(obstacle))
                {
                    rend.material = originalMaterials[obstacle];
                }
            }
        }
        bestPathObstacles.Clear();
        originalMaterials.Clear();
    }

    public static void AddBestPathObstacle(Transform obstacle)
    {
        if (!bestPathObstacles.Contains(obstacle))
        {
            bestPathObstacles.Add(obstacle);
            Renderer rend = obstacle.GetComponent<Renderer>();
            if (rend != null && !originalMaterials.ContainsKey(obstacle))
            {
                originalMaterials.Add(obstacle, rend.material);
            }
        }
    }

    public static Transform GetNearestBestPathObstacle(Vector3 position)
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        foreach (Transform t in bestPathObstacles)
        {
            if (t == null) continue;
            float dist = Vector3.Distance(position, t.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = t;
            }
        }
        return nearest;
    }
}
