using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField][Range(0.001f, 1f)] private float updateInterval = 0.1f;
    [SerializeField][Range(0, 1f)] private float randomFillShare = 0.5f;
    [SerializeField] private RuleZone defaultRules;

    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private InputActionsHandler inputHandler;

    private bool isSimulating = false;
    private float timer = 0f;
    
    public bool IsSimulating => isSimulating;
    public float UpdateInterval => updateInterval;
    public float RandomFillShare => randomFillShare;

    private void Start()
    {
        gridManager = GetComponent<GridManager>();
        zoneManager = GetComponent<RuleZoneManager>();
        inputHandler = GetComponent<InputActionsHandler>();

        gridManager.InitializeGrid();
        gridManager.CreateVisualGrid();
        zoneManager.Initialize(gridManager);
        SetupInputHandlers();
    }

    private void Update()
    {
        if (!isSimulating) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            ComputeNextGeneration();
            gridManager.UpdateGridVisuals();
        }
    }
    
    private void SetupInputHandlers()
    {
        inputHandler.onToggleSimulation.AddListener(ToggleSimulation);
        inputHandler.onRotateStructure.AddListener(OnRotateStructure);
        inputHandler.onMouseMove.AddListener(OnMouseMove);
        inputHandler.onMouseClick.AddListener(OnMouseClick);
        inputHandler.onMouseRightClick.AddListener(OnMouseRightClick);
    }
    
    private void OnRotateStructure()
    {
        if (gridManager.IsPlacingStructure)
            gridManager.RotateStructure();
    }
    
    private void OnMouseClick(Vector2 mousePosition)
    {
        Vector2 worldPos = inputHandler.GetMouseWorldPosition();
        Vector2Int gridPos = WorldToGridPosition(worldPos);
    
        if (gridManager.IsPlacingStructure)
        {
            gridManager.PlaceStructure(gridPos);
        }
        else if (gridManager.IsPlacingZoneStructure)
        {
            gridManager.PlaceZoneStructure(gridPos);
        }
        else
        {
            // Default cell editing in Cells mode
            gridManager.SetCellState(gridPos.x, gridPos.y, true);
        }
    }

    private void OnMouseRightClick(Vector2 mousePosition)
    {
        if (gridManager.IsPlacingStructure)
        {
            gridManager.CancelStructurePlacement();
            return;
        }
        else if (gridManager.IsPlacingZoneStructure)
        {
            gridManager.CancelZoneStructurePlacement();
            return;
        }
    
        // Default cell editing in Cells mode
        Vector2 worldPos = inputHandler.GetMouseWorldPosition();
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        gridManager.SetCellState(gridPos.x, gridPos.y, false);
    }

    private void OnMouseMove(Vector2 mousePosition)
    {
        Vector2 worldPos = inputHandler.GetMouseWorldPosition();
        Vector2Int gridPos = WorldToGridPosition(worldPos);
    
        // Update preview for both structures and zones
        if (gridManager.IsPlacingStructure)
        {
            gridManager.UpdateStructurePreview(gridPos);
        }
        else if (gridManager.IsPlacingZoneStructure)
        {
            gridManager.UpdateZoneStructurePreview(gridPos);
        }
    }
    
    private Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        Vector3 localPos = worldPosition - (Vector2)transform.position;
        int x = Mathf.RoundToInt(localPos.x / gridManager.CellSize);
        int y = Mathf.RoundToInt(localPos.y / gridManager.CellSize);
        return new Vector2Int(x, y);
    }

    public void RandomizeGrid()
    {
        gridManager.RandomizeGrid(randomFillShare);
    }

    public void ToggleSimulation()
    {
        isSimulating = !isSimulating;
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Clamp(interval, 0.1f, 2.0f);
    }

    public void SetRandomFillShare(float fillShare)
    {
        randomFillShare = Mathf.Clamp(fillShare, 0f, 1f);
    }

    private void ComputeNextGeneration()
    {
        bool[,] newGrid = new bool[gridManager.Width, gridManager.Height];

        for (int x = 0; x < gridManager.Width; ++x)
        {
            for (int y = 0; y < gridManager.Height; ++y)
            {
                if (gridManager.IsWall(x, y))
                {
                    newGrid[x, y] = false;
                    continue;
                }
            
                // Get zone for cell - this now uses the properly managed zones
                RuleZone zone = zoneManager.GetZoneForCell(x, y);
                int minSurvive = zone != null ? zone.minSurviveNeighbors : defaultRules.minSurviveNeighbors;
                int maxSurvive = zone != null ? zone.maxSurviveNeighbors : defaultRules.maxSurviveNeighbors;
                int reproduce = zone != null ? zone.reproduceNeighbors : defaultRules.reproduceNeighbors;

                int liveNeighbors = CountLiveNeighbors(x, y);
                bool isAlive = gridManager.Grid[x, y];

                if (isAlive)
                {
                    newGrid[x, y] = liveNeighbors >= minSurvive && 
                                    liveNeighbors <= maxSurvive;
                }
                else
                {
                    newGrid[x, y] = liveNeighbors == reproduce;
                }
            }
        }

        // Apply new generation
        for (int x = 0; x < gridManager.Width; ++x)
        {
            for (int y = 0; y < gridManager.Height; ++y)
            {
                if (!gridManager.IsWall(x, y))
                {
                    gridManager.SetCellState(x, y, newGrid[x, y]);
                }
            }
        }
    }

    private int CountLiveNeighbors(int x, int y)
    {
        int count = 0;

        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == 0 && j == 0) continue;

                int checkX = x + i;
                int checkY = y + j;

                if (!(0 <= checkX && checkX < gridManager.Width && 0 <= checkY && checkY < gridManager.Height))
                {
                    continue;
                }
                
                if (gridManager.IsWall(checkX, checkY))
                    continue;

                if (gridManager.Grid[checkX, checkY])
                {
                    count++;
                }
            }
        }

        return count;
    }
}
