using UnityEngine;

public class LifeController : MonoBehaviour
{
    [SerializeField][Range(0.001f, 1f)] private float updateInterval = 0.1f;
    [SerializeField][Range(0, 1f)] private float randomFillShare = 0.5f;
    [SerializeField] private SimulationRules defaultRules = new SimulationRules();

    private GridManager gridManager;
    private RuleZoneManager zoneManager;
    private InputHandler inputHandler;

    private bool isSimulating = false;
    private float timer = 0f;
    
    public bool IsSimulating => isSimulating;
    public float UpdateInterval => updateInterval;
    public float RandomFillShare => randomFillShare;

    private void Start()
    {
        gridManager = GetComponent<GridManager>();
        zoneManager = GetComponent<RuleZoneManager>();
        inputHandler = GetComponent<InputHandler>();

        gridManager.InitializeGrid();
        gridManager.CreateVisualGrid();
        zoneManager.Initialize(gridManager);
        inputHandler.Initialize(Camera.main, gridManager, zoneManager, this);
    }

    private void Update()
    {
        inputHandler.HandleInput();
        
        if (!isSimulating) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            ComputeNextGeneration();
            gridManager.UpdateGridVisuals();
        }
    }

    public void RandomizeGrid()
    {
        gridManager.RandomizeGrid(randomFillShare);
    }

    public void StartSimulation()
    {
        isSimulating = true;
        Debug.Log("Simulation Started");
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

        for (int x = 0; x < gridManager.Width; ++x)
        {
            for (int y = 0; y < gridManager.Height; ++y)
            {
                gridManager.SetCellState(x, y, newGrid[x, y]);
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

                if (gridManager.Grid[checkX, checkY])
                {
                    count++;
                }
            }
        }

        return count;
    }

    [ContextMenu("Recreate Grid")]
    public void RecreateGrid()
    {
        gridManager.RecreateGrid();
    }
}
