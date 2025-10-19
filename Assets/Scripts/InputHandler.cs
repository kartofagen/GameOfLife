using UnityEngine;

public class InputHandler : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private LayerMask gridLayerMask = 1;
    [SerializeField] private bool useRaycast = true;

    private Camera mainCamera;
    private Plane gridPlane;
    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private LifeController lifeController;

    private RuleZone currentDrawingZone = null;
    private bool isDrawingZone = false;

    private int selectedStructureIndex = -1;

    public void Initialize(Camera camera, GridManager gridManager, RuleZoneManager zoneManager, LifeController lifeController)
    {
        mainCamera = camera;
        this.gridManager = gridManager;
        this.zoneManager = zoneManager;
        this.lifeController = lifeController;
        gridPlane = new Plane(Vector3.forward, Vector3.zero);
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lifeController.ToggleSimulation();
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isDrawingZone)
                FinishDrawingZone();
            else if (!gridManager.IsPlacingStructure)
                StartDrawingZone();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (gridManager.IsPlacingStructure)
                gridManager.CancelStructurePlacement();
            else if (isDrawingZone)
                CancelDrawingZone();
        }

        if (Input.GetKeyDown(KeyCode.R) && gridManager.IsPlacingStructure)
        {
            gridManager.RotateStructure();
        }

        // Number keys for structure selection
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectStructure(i);
            }
        }

        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector2Int gridPos = GetGridPositionFromMouse(mousePosition);

        if (gridPos.x < 0 || gridPos.y < 0) return;

        // Update structure preview
        if (gridManager.IsPlacingStructure)
        {
            gridManager.UpdateStructurePreview(gridPos);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (gridManager.IsPlacingStructure)
            {
                gridManager.PlaceStructure(gridPos);
            }
            else if (isDrawingZone)
            {
                HandleZoneDrawing(gridPos);
            }
            else
            {
                HandleCellClick(gridPos, true);
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (!gridManager.IsPlacingStructure && !isDrawingZone)
            {
                HandleCellClick(gridPos, false);
            }
        }
        else if (Input.GetMouseButton(0) && !gridManager.IsPlacingStructure && !isDrawingZone)
        {
            HandleCellClick(gridPos, true);
        }
        else if (Input.GetMouseButton(1) && !gridManager.IsPlacingStructure && !isDrawingZone)
        {
            HandleCellClick(gridPos, false);
        }
    }

    private Vector2Int GetGridPositionFromMouse(Vector3 mousePosition)
    {
        if (useRaycast)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            {
                return WorldToGridPosition(hit.point);
            }
        }
        else
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            float distance;
            
            if (gridPlane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                return WorldToGridPosition(worldPos);
            }
        }

        return new Vector2Int(-1, -1);
    }

    private Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - transform.position.x) / gridManager.CellSize);
        int y = Mathf.RoundToInt((worldPos.y - transform.position.y) / gridManager.CellSize);
        return new Vector2Int(x, y);
    }

    private void HandleCellClick(Vector2Int gridPos, bool state)
    {
        if (gridPos.x >= 0 && gridPos.x < gridManager.Width && gridPos.y >= 0 && gridPos.y < gridManager.Height)
        {
            if (gridManager.GetCellState(gridPos.x, gridPos.y) != state)
            {
                gridManager.SetCellState(gridPos.x, gridPos.y, state);
            }
        }
    }

    private void HandleZoneDrawing(Vector2Int gridPos)
    {
        if (gridPos.x >= 0 && gridPos.x < gridManager.Width && gridPos.y >= 0 && gridPos.y < gridManager.Height)
        {
            bool newState = Input.GetMouseButton(0);
            if (Input.GetMouseButton(2))
            {
                newState = true;
            }

            if (currentDrawingZone.zoneCells[gridPos.x, gridPos.y] != newState)
            {
                currentDrawingZone.zoneCells[gridPos.x, gridPos.y] = newState;
                UpdateTemporaryZoneVisual(gridPos.x, gridPos.y, newState);
            }
        }
    }

    public void SelectStructure(int index)
    {
        if (index < 0 || index >= gridManager.AvailableStructures.Count) return;
        
        if (isDrawingZone)
            CancelDrawingZone();

        StructureData structure = gridManager.AvailableStructures[index];
    
        // Проверяем инициализацию
        structure.InitializeCells();
        if (structure.cells == null)
        {
            Debug.LogError($"Structure {structure.structureName} failed to initialize cells!");
            return;
        }
    
        gridManager.StartStructurePlacement(structure);
        Debug.Log($"Selected structure: {structure.structureName} ({structure.width}x{structure.height})");
    }

    private void CancelDrawingZone()
    {
        isDrawingZone = false;
        currentDrawingZone = null;
        zoneManager.ClearTemporaryZoneVisual();
        gridManager.UpdateGridVisuals();
        Debug.Log("Zone drawing canceled");
    }

    private void StartDrawingZone()
    {
        isDrawingZone = true;
        currentDrawingZone = ScriptableObject.CreateInstance<RuleZone>();
        currentDrawingZone.name = $"Zone {zoneManager.RuleZones.Count + 1}";
        currentDrawingZone.zoneCells = new bool[gridManager.Width, gridManager.Height];
        currentDrawingZone.zoneColor = new Color(Random.value, Random.value, Random.value, 0.3f);
    
        Debug.Log("Started drawing custom rule zone. Press Z again to finish.");
    }

    private void FinishDrawingZone()
    {
        if (!isDrawingZone || currentDrawingZone == null) return;
    
        bool hasCells = false;
        for (int x = 0; x < gridManager.Width; ++x)
        {
            for (int y = 0; y < gridManager.Height; ++y)
            {
                if (currentDrawingZone.zoneCells[x, y])
                {
                    hasCells = true;
                    break;
                }
            }
        }
    
        if (hasCells)
        {
            zoneManager.AddZone(currentDrawingZone);
            Debug.Log($"Finished drawing zone '{currentDrawingZone.name}'");
        }
        else
        {
            Debug.Log("Zone drawing canceled - no cells selected");
        }
    
        isDrawingZone = false;
        currentDrawingZone = null;
        zoneManager.ClearTemporaryZoneVisual();
        gridManager.UpdateGridVisuals();
    }

    private void HandleMouseDrawing()
    {
        Vector3 mousePosition = Input.mousePosition;
        
        if (useRaycast)
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, gridLayerMask))
            {
                ProcessCellAtWorldPosition(hit.point);
            }
        }
        else
        {
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            float distance;
            
            if (gridPlane.Raycast(ray, out distance))
            {
                Vector3 worldPos = ray.GetPoint(distance);
                ProcessCellAtWorldPosition(worldPos);
            }
        }
    }

    private void ProcessCellAtWorldPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - transform.position.x) / gridManager.CellSize);
        int y = Mathf.RoundToInt((worldPos.y - transform.position.y) / gridManager.CellSize);

        if (x >= 0 && x < gridManager.Width && y >= 0 && y < gridManager.Height)
        {
            if (isDrawingZone)
            {
                bool newState = Input.GetMouseButton(0);
                if (Input.GetMouseButton(2))
                {
                    newState = true;
                }

                if (currentDrawingZone.zoneCells[x, y] != newState)
                {
                    currentDrawingZone.zoneCells[x, y] = newState;
                    UpdateTemporaryZoneVisual(x, y, newState);
                }
            }
            else
            {
                bool newState = Input.GetMouseButton(0);
                if (gridManager.GetCellState(x, y) != newState)
                {
                    gridManager.SetCellState(x, y, newState);
                }
            }
        }
    }

    private void UpdateTemporaryZoneVisual(int x, int y, bool state)
    {
        zoneManager.UpdateTemporaryZoneVisual(currentDrawingZone);
    }
}