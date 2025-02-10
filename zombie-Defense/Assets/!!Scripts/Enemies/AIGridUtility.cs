using System.Collections.Generic;
using UnityEngine;

public class AIGridUtility : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform player;

    [Header("Grid Reference")]
    [SerializeField] private GridCubePlacer gridPlacer;

    [Header("Debug")]
    [SerializeField] private bool showGridDebug = true;

    [ContextMenu("Check Player Surround")]
    public void CheckPlayerSurround()
    {
        if (gridPlacer == null)
        {
            Debug.LogError("GridCubePlacer reference is not set!");
            return;
        }

        int playerCellX = GetPlayerCellX();
        int playerCellZ = GetPlayerCellZ();

        if (IsPlayerSurrounded(playerCellX, playerCellZ))
        {
            Debug.Log("Player is completely surrounded by obstacles.");
        }
        else
        {
            Debug.Log("Player is not completely surrounded by obstacles.");
        }

        if (showGridDebug)
        {
            DebugGridCells();
        }
    }

    [ContextMenu("Test Player Surround")]
    public void TestPlayerSurround()
    {
        Debug.Log("Testing player surround condition...");
        CheckPlayerSurround();
    }

    public int GetPlayerCellX()
    {
        Vector3 gridOrigin = gridPlacer.transform.position - new Vector3(gridPlacer.gridWidth * gridPlacer.cellSize, 0, gridPlacer.gridHeight * gridPlacer.cellSize) * 0.5f;
        Vector3 offset = player.position - gridOrigin;
        return Mathf.FloorToInt(offset.x / gridPlacer.cellSize);
    }

   public int GetPlayerCellZ()
    {
        Vector3 gridOrigin = gridPlacer.transform.position - new Vector3(gridPlacer.gridWidth * gridPlacer.cellSize, 0, gridPlacer.gridHeight * gridPlacer.cellSize) * 0.5f;
        Vector3 offset = player.position - gridOrigin;
        return Mathf.FloorToInt(offset.z / gridPlacer.cellSize);
    }

    public bool IsPlayerSurrounded(int cellX, int cellZ)
    {
        if (cellX < 0 || cellX >= gridPlacer.gridWidth || cellZ < 0 || cellZ >= gridPlacer.gridHeight)
        {
            Debug.LogWarning("Player is out of grid bounds!");
            return false;
        }

        Queue<Vector2Int> cellsToCheck = new Queue<Vector2Int>();
        cellsToCheck.Enqueue(new Vector2Int(cellX, cellZ));

        bool[,] visited = new bool[gridPlacer.gridWidth, gridPlacer.gridHeight];
        visited[cellX, cellZ] = true;

        while (cellsToCheck.Count > 0)
        {
            Vector2Int current = cellsToCheck.Dequeue();

            if (IsAtEdge(current.x, current.y))
            {
                return false;
            }

            foreach (Vector2Int dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                int checkX = current.x + dir.x;
                int checkZ = current.y + dir.y;

                if (checkX >= 0 && checkX < gridPlacer.gridWidth && 
                    checkZ >= 0 && checkZ < gridPlacer.gridHeight && 
                    !gridPlacer.occupiedCells[checkX, checkZ] && 
                    !visited[checkX, checkZ])
                {
                    visited[checkX, checkZ] = true;
                    cellsToCheck.Enqueue(new Vector2Int(checkX, checkZ));
                }
            }
        }

        return true;
    }

    private bool IsAtEdge(int x, int z)
    {
        return x == 0 || x == gridPlacer.gridWidth - 1 || z == 0 || z == gridPlacer.gridHeight - 1;
    }
    private void DebugGridCells()
    {
        for (int x = 0; x < gridPlacer.gridWidth; x++)
        {
            for (int z = 0; z < gridPlacer.gridHeight; z++)
            {
                Debug.Log($"Cell [{x}, {z}] is {(gridPlacer.occupiedCells[x, z] ? "Occupied" : "Free")}");
            }
        }
    }
}