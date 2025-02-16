using UnityEngine;

public class LandmineOccupant : MonoBehaviour
{
    public int cellX;
    public int cellZ;

    public void UnoccupyCell()
    {
        if (GridCubePlacer.Instance != null)
        {
            GridCubePlacer.Instance.Unoccupy(cellX, cellZ);
        }
    }
}