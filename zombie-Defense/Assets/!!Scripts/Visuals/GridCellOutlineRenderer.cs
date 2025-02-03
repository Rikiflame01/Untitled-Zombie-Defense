using System.Collections.Generic;
using UnityEngine;

public class GridCellOutlineRenderer : MonoBehaviour
{

    [Header("Grid Settings")]
    public int gridWidth = 10;       
    public int gridHeight = 10;     
    public float cellSize = 1f;     

    [Header("Appearance")]
    public Color lineColor = Color.green;
    public float lineWidth = 0.1f;   
    public Material lineMaterial;     

    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr != null)
                Destroy(lr.gameObject);
        }
        lineRenderers.Clear();

        Vector3 origin = transform.position - new Vector3(gridWidth * cellSize, 0, gridHeight * cellSize) * 0.5f;

        for (int x = 0; x <= gridWidth; x++)
        {
            GameObject lineObj = new GameObject("VerticalLine_" + x);
            lineObj.transform.parent = transform;
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;

            Vector3 start = origin + new Vector3(x * cellSize, 0, 0);
            Vector3 end   = origin + new Vector3(x * cellSize, 0, gridHeight * cellSize);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            lineRenderers.Add(lr);
        }

        for (int z = 0; z <= gridHeight; z++)
        {
            GameObject lineObj = new GameObject("HorizontalLine_" + z);
            lineObj.transform.parent = transform;
            LineRenderer lr = lineObj.AddComponent<LineRenderer>();
            lr.material = lineMaterial;
            lr.startColor = lineColor;
            lr.endColor = lineColor;
            lr.startWidth = lineWidth;
            lr.endWidth = lineWidth;
            lr.positionCount = 2;

            Vector3 start = origin + new Vector3(0, 0, z * cellSize);
            Vector3 end   = origin + new Vector3(gridWidth * cellSize, 0, z * cellSize);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);

            lineRenderers.Add(lr);
        }
    }
}

