using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridCubePlacer : MonoBehaviour
{
    public static GridCubePlacer Instance { get; private set; }

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

    [Tooltip("Prefab to show as a 'ghosted' preview under the mouse.")]
    public GameObject previewPrefab;

    [Tooltip("Optional parent transform for placed cubes (leave null if you don't want any parent).")]
    public Transform gridParent;

    [Tooltip("Y coordinate of the grid (the plane on which the cubes are placed).")]
    public float gridY = 0f;

    [Tooltip("Cube will be scaled by this factor relative to cellSize (e.g., 0.9 for a little margin).")]
    [Range(0f, 1f)]
    public float cubeMargin = 0.9f;

    [Header("Camera Settings")]
    [Tooltip("The camera used for raycasting when not using a render texture. If left empty, Camera.main will be used.")]
    public Camera mainCamera;

    [Header("Render Texture Settings")]
    [Tooltip("The camera rendering to the render texture. If using a render texture for input, assign this camera.")]
    public Camera renderTextureCamera;
    [Tooltip("The Render Texture used for displaying the game.")]
    public RenderTexture renderTexture;

    private InputAction clickAction;
    private GameObject previewInstance;
    public bool[,] occupiedCells;

    private Renderer previewRenderer;
    private Color validPreviewColor = new Color(1f, 1f, 1f, 0.5f);
    private Color invalidPreviewColor = new Color(1f, 0f, 0f, 0.5f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
            mainCamera = Camera.main;

        clickAction = new InputAction("PlaceCube", binding: "<Mouse>/leftButton");
        clickAction.performed += OnPlaceCube;

        occupiedCells = new bool[gridWidth, gridHeight];
    }

    public void Unoccupy(int x, int z)
    {
        occupiedCells[x, z] = false;
    }

    private void OnEnable()
    {
        clickAction.Enable();

        if (previewPrefab != null)
        {
            previewInstance = Instantiate(previewPrefab);
            previewInstance.SetActive(false);

            previewRenderer = previewInstance.GetComponentInChildren<Renderer>();
            if (previewRenderer != null)
            {
                previewRenderer.material.color = validPreviewColor;
            }
        }
    }

    private void OnDisable()
    {
        clickAction.Disable();
    }

    private void Update()
    {
        if (GameStateManager.CurrentState == GameStateManager.GameState.Building && previewInstance != null)
        {
            UpdatePreviewPosition();
        }
        else if (previewInstance != null)
        {
            previewInstance.SetActive(false);
        }
    }

    private Ray GetRayFromMouse()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        if (renderTexture != null && renderTextureCamera != null)
        {
            float scaleX = (float)renderTexture.width / Screen.width;
            float scaleY = (float)renderTexture.height / Screen.height;
            Vector2 renderMousePos = new Vector2(mousePos.x * scaleX, mousePos.y * scaleY);
            return renderTextureCamera.ScreenPointToRay(new Vector3(renderMousePos.x, renderMousePos.y, 0));
        }
        else
        {
            return mainCamera.ScreenPointToRay(mousePos);
        }
    }

    private void UpdatePreviewPosition()
    {
        Ray ray = GetRayFromMouse();

        if (Mathf.Approximately(ray.direction.y, 0f))
        {
            previewInstance.SetActive(false);
            return;
        }

        float t = -(ray.origin.y - gridY) / ray.direction.y;
        if (t < 0)
        {
            previewInstance.SetActive(false);
            return;
        }

        Vector3 hitPoint = ray.origin + t * ray.direction;
        Vector3 gridOrigin = transform.position - new Vector3(gridWidth * cellSize, 0, gridHeight * cellSize) * 0.5f;
        Vector3 offset = hitPoint - gridOrigin;
        int cellX = Mathf.FloorToInt(offset.x / cellSize);
        int cellZ = Mathf.FloorToInt(offset.z / cellSize);

        bool inBounds = (cellX >= 0 && cellX < gridWidth && cellZ >= 0 && cellZ < gridHeight);
        bool isOccupied = inBounds && occupiedCells[cellX, cellZ];

        previewInstance.SetActive(true);

        if (!inBounds)
        {
            previewInstance.transform.position = hitPoint;
            SetPreviewColor(invalidPreviewColor);
        }
        else
        {
            float centerX = gridOrigin.x + (cellX + 0.5f) * cellSize;
            float centerZ = gridOrigin.z + (cellZ + 0.5f) * cellSize;
            float cubeSize = cellSize * cubeMargin;
            float cubeCenterY = gridY + (cubeSize * 0.5f);

            Vector3 cellCenter = new Vector3(centerX, cubeCenterY, centerZ);
            previewInstance.transform.position = cellCenter;
            previewInstance.transform.localScale = new Vector3(0.8f, 1f, 0.8f);

            SetPreviewColor(isOccupied ? invalidPreviewColor : validPreviewColor);
        }
    }

    private void SetPreviewColor(Color color)
    {
        if (previewRenderer != null)
        {
            previewRenderer.material.color = color;
        }
    }

    private void OnPlaceCube(InputAction.CallbackContext context)
    {
        if (GameStateManager.CurrentState != GameStateManager.GameState.Building)
            return;

        Ray ray = GetRayFromMouse();

        if (Mathf.Approximately(ray.direction.y, 0f))
            return;

        float t = -(ray.origin.y - gridY) / ray.direction.y;
        if (t < 0)
            return;

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

        if (occupiedCells[cellX, cellZ])
        {
            Debug.Log("Cannot place here, cell is occupied.");
            return;
        }

        int cost = 1;
        if (ScoreManager.Instance.totalScore < cost)
        {
            Debug.Log("Not enough score to place this object.");
            return;
        }

        float centerX = gridOrigin.x + (cellX + 0.5f) * cellSize;
        float centerZ = gridOrigin.z + (cellZ + 0.5f) * cellSize;
        float cubeSize = cellSize * cubeMargin;
        float cubeCenterY = gridY + (cubeSize * 0.5f);
        Vector3 cellCenter = new Vector3(centerX, cubeCenterY, centerZ);

        ScoreManager.Instance.DecreaseTotalScore(cost);

        GameObject cube = Instantiate(cubePrefab, cellCenter, Quaternion.identity);
        
        // Start the scaling animation
        StartCoroutine(AnimatePlacement(cube));

        occupiedCells[cellX, cellZ] = true;

        var occupant = cube.AddComponent<WallOccupant>();
        occupant.cellX = cellX;
        occupant.cellZ = cellZ;
        SoundManager.Instance.PlaySFX("placeCrate", 1f);
    }

    private IEnumerator AnimatePlacement(GameObject cube)
    {
        Vector3 normalScale = new Vector3(1.15f, 1.15f, 1.15f);  // Intended final scale
        Vector3 enlargedScale = normalScale * 1.3f;      // 30% larger for effect
        float duration = 0.15f; // Time for each phase

        float time = 0f;

        // Scale up effect
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            cube.transform.localScale = Vector3.Lerp(normalScale, enlargedScale, progress);
            yield return null;
        }

        time = 0f;

        // Scale back down to normal
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            cube.transform.localScale = Vector3.Lerp(enlargedScale, normalScale, progress);
            yield return null;
        }

        cube.transform.localScale = normalScale;
    }
}
