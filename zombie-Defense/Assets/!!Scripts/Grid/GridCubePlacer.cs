using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GridCubePlacer : MonoBehaviour
{
    public static GridCubePlacer Instance { get; private set; }
    public enum PlacementType { Wall, Landmine }
    public PlacementType currentPlacement = PlacementType.Wall;

    public int gridWidth = 20;
    public int gridHeight = 20;
    public float cellSize = 1f;

    public GameObject wallPrefab;
    public GameObject landminePrefab;

    public GameObject wallPreviewPrefab;
    public GameObject landminePreviewPrefab;

    public Transform gridParent;
    public float gridY = 0f;
    [Range(0f, 1f)]
    public float cubeMargin = 0.9f;

    public Camera mainCamera;
    public Camera renderTextureCamera;
    public RenderTexture renderTexture;

    public TMP_Text buildModeInstructionsText;
    public GameObject buildModePanel;

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
        clickAction = new InputAction("PlaceObject", binding: "<Mouse>/leftButton");
        clickAction.performed += OnPlaceObject;
        occupiedCells = new bool[gridWidth, gridHeight];
    }

    public void Unoccupy(int x, int z)
    {
        occupiedCells[x, z] = false;
    }

    private void OnEnable()
    {
        clickAction.Enable();
        CreatePreviewInstance();
    }

    private void OnDisable()
    {
        clickAction.Disable();
    }

    private void Update()
    {
        if (GameStateManager.CurrentState == GameStateManager.GameState.Building)
        {
            UpdateBuildModeUI();
            UpdatePreviewPosition();
            if (Keyboard.current.tKey.wasPressedThisFrame)
                TogglePlacement();
        }
        else
        {
            if (previewInstance != null)
                previewInstance.SetActive(false);

            if (buildModePanel != null && buildModePanel.activeSelf)
                buildModePanel.SetActive(false);
        }
    }

    private void UpdateBuildModeUI()
    {
        if (buildModePanel == null || buildModeInstructionsText == null)
            return;

        if (!buildModePanel.activeSelf)
            buildModePanel.SetActive(true);

        string instruction = "BUILDING MODE ACTIVE\n";
        instruction += "Press 'T' to swap placement\n";
        instruction += $"Currently Selected: {currentPlacement}\n";
        instruction += (currentPlacement == PlacementType.Landmine) ? "Cost: 5" : "Cost: 1";

        buildModeInstructionsText.text = instruction;
    }

    private void TogglePlacement()
    {
        if (currentPlacement == PlacementType.Wall)
            currentPlacement = PlacementType.Landmine;
        else
            currentPlacement = PlacementType.Wall;
        if (previewInstance != null)
            Destroy(previewInstance);
        CreatePreviewInstance();
    }

    private void CreatePreviewInstance()
    {
        GameObject prefabToUse = (currentPlacement == PlacementType.Wall) ? wallPreviewPrefab : landminePreviewPrefab;
        if (prefabToUse != null)
        {
            previewInstance = Instantiate(prefabToUse);
            previewInstance.SetActive(false);
            previewRenderer = previewInstance.GetComponentInChildren<Renderer>();
            if (previewRenderer != null)
                previewRenderer.material.color = validPreviewColor;
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
            float objectSize = cellSize * cubeMargin;
            float objectCenterY = gridY + (objectSize * 0.5f);
            Vector3 cellCenter = new Vector3(centerX, objectCenterY, centerZ);
            previewInstance.transform.position = cellCenter;
            if (currentPlacement == PlacementType.Wall)
                previewInstance.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            else
                previewInstance.transform.localScale = new Vector3(0.00376667967f, 0.00376667967f, 0.00376667967f);
            SetPreviewColor(isOccupied ? invalidPreviewColor : validPreviewColor);
        }
    }

    private void SetPreviewColor(Color color)
    {
        if (previewRenderer != null)
            previewRenderer.material.color = color;
    }

    public void SetPlacementType(PlacementType newType)
    {
        currentPlacement = newType;
        if (previewInstance != null)
            Destroy(previewInstance);
        CreatePreviewInstance();
    }

    private void OnPlaceObject(InputAction.CallbackContext context)
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
            return;
        if (occupiedCells[cellX, cellZ])
            return;
        int cost = (currentPlacement == PlacementType.Landmine) ? 5 : 1;
        if (ScoreManager.Instance.totalScore < cost)
            return;
        float centerX = gridOrigin.x + (cellX + 0.5f) * cellSize;
        float centerZ = gridOrigin.z + (cellZ + 0.5f) * cellSize;
        float objectSize = cellSize * cubeMargin;
        float objectCenterY = gridY + (objectSize * 0.5f);
        Vector3 cellCenter = new Vector3(centerX, objectCenterY, centerZ);
        ScoreManager.Instance.DecreaseTotalScore(cost);
        GameObject placedObject = null;
        if (currentPlacement == PlacementType.Wall)
        {
            placedObject = Instantiate(wallPrefab, cellCenter, Quaternion.identity);
            var occupant = placedObject.AddComponent<WallOccupant>();
            occupant.cellX = cellX;
            occupant.cellZ = cellZ;
            StartCoroutine(AnimatePlacement(placedObject));
        }
        else if (currentPlacement == PlacementType.Landmine)
        {
            placedObject = Instantiate(landminePrefab, cellCenter, Quaternion.identity);
            var occupant = placedObject.AddComponent<LandmineOccupant>();
            occupant.cellX = cellX;
            occupant.cellZ = cellZ;
            placedObject.transform.localScale = new Vector3(0.00376667967f, 0.00376667967f, 0.00376667967f);
        }
        occupiedCells[cellX, cellZ] = true;
        SoundManager.Instance.PlaySFX("placeCrate", 1f);
    }

    private IEnumerator AnimatePlacement(GameObject obj)
    {
        Vector3 normalScale = new Vector3(1.15f, 1.15f, 1.15f);
        Vector3 enlargedScale = normalScale * 1.3f;
        float duration = 0.15f;
        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            obj.transform.localScale = Vector3.Lerp(normalScale, enlargedScale, progress);
            yield return null;
        }
        time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            obj.transform.localScale = Vector3.Lerp(enlargedScale, normalScale, progress);
            yield return null;
        }
        obj.transform.localScale = normalScale;
    }
}
