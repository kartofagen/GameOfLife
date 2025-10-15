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
            else
                StartDrawingZone();
        }

        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            HandleMouseDrawing();
        }
    }

    private void StartDrawingZone()
    {
        isDrawingZone = true;
        currentDrawingZone = new RuleZone();
        currentDrawingZone.name = $"Zone {zoneManager.RuleZones.Count + 1}";
        currentDrawingZone.zoneCells = new bool[gridManager.Width, gridManager.Height];
        
        Debug.Log("Started drawing custom rule zone. Press Z again to finish.");
    }

    private void FinishDrawingZone()
    {
        if (!isDrawingZone || currentDrawingZone == null) return;
        
        bool hasCells = false;
        for (int x = 0; x < gridManager.Width; x++)
        {
            for (int y = 0; y < gridManager.Height; y++)
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
        // ...
    }
}
