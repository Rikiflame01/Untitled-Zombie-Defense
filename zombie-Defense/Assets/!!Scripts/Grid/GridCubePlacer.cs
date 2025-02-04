using UnityEngine;
using UnityEngine.InputSystem;

public class GridCubePlacer : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("Number of cells along the X axis")]
    public int gridWidth = 20;
    [Tooltip("Number of cells along the Z axis")]
    public int gridHeight = 20;
    [Tooltip("Size of each cell (assumes square cells)")]
    public float cellSize = 1f;
    
    [Header("Placement Settings")]
    [Tooltip("Prefab for the cube to place. Its pivot should be centered.")]
    public GameObject cubePrefab;
    [Tooltip("Optional parent transform for placed cubes (leave null if you don't want any parent).")]
    public Transform gridParent;
    [Tooltip("Y coordinate of the grid (the plane on which the cubes are placed).")]
    public float gridY = 0f;
    [Tooltip("Cube will be scaled by this factor relative to cellSize (e.g., 0.9 for a little margin).")]
    [Range(0f, 1f)]
    public float cubeMargin = 0.9f;

    [Header("References")]
    [Tooltip("The camera used for raycasting. If left empty, Camera.main will be used.")]
    public Camera mainCamera;

    private InputAction clickAction;

    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        clickAction = new InputAction("PlaceCube", binding: "<Mouse>/leftButton");
        clickAction.performed += OnPlaceCube;
    }

    private void OnEnable()
    {
        clickAction.Enable();
    }

    private void OnDisable()
    {
        clickAction.Disable();
    }

    private void OnPlaceCube(InputAction.CallbackContext context)
    {

        if (GameStateManager.CurrentState != GameStateManager.GameState.Building)
        {
            Debug.Log("Current state is not Building");
            return;
        }


        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);


        if (Mathf.Approximately(ray.direction.y, 0f)) return;


        float t = -(ray.origin.y - gridY) / ray.direction.y;
        if (t < 0) return;

        Vector3 hitPoint = ray.origin + t * ray.direction;


        Vector3 gridOrigin = transform.position - new Vector3(gridWidth * cellSize, 0, gridHeight * cellSize) * 0.5f;
        Vector3 offset = hitPoint - gridOrigin;
        int cellX = Mathf.FloorToInt(offset.x / cellSize);
        int cellZ = Mathf.FloorToInt(offset.z / cellSize);

        if (cellX < 0 || cellX >= gridWidth || cellZ < 0 || cellZ >= gridHeight)
        {
            Debug.Log("Clicked outside grid bounds.");
            return;
        }

        float centerX = gridOrigin.x + (cellX + 0.5f) * cellSize;
        float centerZ = gridOrigin.z + (cellZ + 0.5f) * cellSize;
        float cubeSize = cellSize * cubeMargin;
        float cubeCenterY = gridY + (cubeSize * 0.5f);

        Vector3 cellCenter = new Vector3(centerX, cubeCenterY, centerZ);

        GameObject cube = Instantiate(cubePrefab, cellCenter, Quaternion.identity);
        
        cube.transform.localScale = new Vector3(0.8f, 1, 0.8f);

    }
}
