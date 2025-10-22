using UnityEngine;
using System.Collections;

public class LifeController : MonoBehaviour
{
    [SerializeField] [Range(0.001f, 1f)] private float updateInterval = 0.1f;
    [SerializeField] [Range(0, 1f)] private float randomFillShare = 0.5f;
    [SerializeField] private RuleZone defaultRules;

    private GridManager _gridManager;
    private RuleZoneManager _zoneManager;
    private InputActionsHandler _inputHandler;
    private TournamentManager _tournamentManager;

    private bool _isSimulating = false;
    private float _timer = 0f;
    
    private bool _canPlaceZone = true;
    private float _zonePlacementCooldown = 0.3f;

    public bool IsSimulating => _isSimulating;
    public float UpdateInterval => updateInterval;
    public float RandomFillShare => randomFillShare;

    private void Start()
    {
        _gridManager = GetComponent<GridManager>();
        _zoneManager = GetComponent<RuleZoneManager>();
        _inputHandler = GetComponent<InputActionsHandler>();
        _tournamentManager = GetComponent<TournamentManager>();

        _gridManager.InitializeGrid();
        _gridManager.CreateVisualGrid();
        _zoneManager.Initialize(_gridManager);
        SetupInputHandlers();
    }

    private void Update()
    {
        if (!_isSimulating) return;

        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0f;
            ComputeNextGeneration();
            _gridManager.UpdateGridVisuals();
        }
    }

    private void SetupInputHandlers()
    {
        _inputHandler.onToggleSimulation.AddListener(ToggleSimulation);
        _inputHandler.onRotateStructure.AddListener(OnRotateStructure);
        _inputHandler.onMouseMove.AddListener(OnMouseMove);
        _inputHandler.onMouseClick.AddListener(OnMouseClick);
        _inputHandler.onMouseRightClick.AddListener(OnMouseRightClick);
    }

    private void OnRotateStructure()
    {
        if (_gridManager.IsPlacingStructure)
            _gridManager.RotateStructure();
    }

    private void OnMouseClick(Vector2 mousePosition)
    {
        Vector2 worldPos = _inputHandler.GetMouseWorldPosition();
        Vector2Int gridPos = WorldToGridPosition(worldPos);

        if (_tournamentManager != null && _tournamentManager.IsTournament)
        {
            if (_tournamentManager.CurrentState == TournamentState.Player1Zones && 
                _gridManager.IsPlacingZoneStructure)
            {
                if (_canPlaceZone && _gridManager.PlaceZoneStructure(gridPos) && _tournamentManager.CanPlaceZone())
                {
                    _tournamentManager.OnZonePlaced();
                    StartCoroutine(ZonePlacementCooldown());
                }
            }
            else if (_tournamentManager.CurrentState == TournamentState.Player2Cells)
            {
                if (_gridManager.IsPlacingStructure)
                {
                    int cellsAdded = _gridManager.PlaceStructure(gridPos);
                
                    for (int i = 0; i < cellsAdded && _tournamentManager.CanPlaceCell(); ++i)
                    {
                        _tournamentManager.OnCellPlaced();
                    }
                }
                else if (!_gridManager.IsWall(gridPos.x, gridPos.y) && _tournamentManager.CanPlaceCell())
                {
                    if (!_gridManager.GetCellState(gridPos.x, gridPos.y))
                    {
                        _gridManager.SetCellState(gridPos.x, gridPos.y, true);
                        _tournamentManager.OnCellPlaced();
                    }
                }
            }
        }
        else
        {
            if (_gridManager.IsPlacingStructure)
            {
                _gridManager.PlaceStructure(gridPos);
            }
            else if (_gridManager.IsPlacingZoneStructure)
            {
                if (_canPlaceZone)
                {
                    _gridManager.PlaceZoneStructure(gridPos);
                    StartCoroutine(ZonePlacementCooldown());
                }
            }
            else
            {
                _gridManager.SetCellState(gridPos.x, gridPos.y, true);
            }
        }
    }
    
    private IEnumerator ZonePlacementCooldown()
    {
        _canPlaceZone = false;
        yield return new WaitForSeconds(_zonePlacementCooldown);
        _canPlaceZone = true;
    }

    private void OnMouseRightClick(Vector2 mousePosition)
    {
        if (_tournamentManager != null && _tournamentManager.IsTournament)
        {
            Vector2 worldPos = _inputHandler.GetMouseWorldPosition();
            Vector2Int gridPos = WorldToGridPosition(worldPos);

            if (_tournamentManager.CurrentState == TournamentState.Player1Zones)
            {
                RuleZoneManager zoneManager = GetComponent<RuleZoneManager>();
                if (zoneManager != null && zoneManager.RemoveZoneAtPosition(gridPos))
                {
                    _tournamentManager.OnZoneRemoved();
                }
            }
            else if (_tournamentManager.CurrentState == TournamentState.Player2Cells)
            {
                if (_gridManager.GetCellState(gridPos.x, gridPos.y))
                {
                    _gridManager.SetCellState(gridPos.x, gridPos.y, false);
                    _tournamentManager.OnCellRemoved();
                }
            }
        
            if (_gridManager.IsPlacingStructure)
            {
                _gridManager.CancelStructurePlacement();
            }
            else if (_gridManager.IsPlacingZoneStructure)
            {
                _gridManager.CancelZoneStructurePlacement();
            }
        }
        else
        {
            if (_gridManager.IsPlacingStructure)
            {
                _gridManager.CancelStructurePlacement();
                return;
            }
            else if (_gridManager.IsPlacingZoneStructure)
            {
                _gridManager.CancelZoneStructurePlacement();
                return;
            }

            Vector2 worldPos = _inputHandler.GetMouseWorldPosition();
            Vector2Int gridPos = WorldToGridPosition(worldPos);
            _gridManager.SetCellState(gridPos.x, gridPos.y, false);
        }
    }

    private void OnMouseMove(Vector2 mousePosition)
    {
        Vector2 worldPos = _inputHandler.GetMouseWorldPosition();
        Vector2Int gridPos = WorldToGridPosition(worldPos);
    
        if (_gridManager.IsPlacingStructure)
        {
            _gridManager.UpdateStructurePreview(gridPos);
        }
        else if (_gridManager.IsPlacingZoneStructure)
        {
            _gridManager.UpdateZoneStructurePreview(gridPos);
        }
    }
    
    private Vector2Int WorldToGridPosition(Vector2 worldPosition)
    {
        Vector3 localPos = worldPosition - (Vector2)transform.position;
        int x = Mathf.RoundToInt(localPos.x / _gridManager.CellSize);
        int y = Mathf.RoundToInt(localPos.y / _gridManager.CellSize);
        return new Vector2Int(x, y);
    }

    public void RandomizeGrid()
    {
        _gridManager.RandomizeGrid(randomFillShare);
    }

    public void ToggleSimulation()
    {
        if (!_tournamentManager.IsTournament || _tournamentManager.CurrentState == TournamentState.Simulating)
        {
            _isSimulating = !_isSimulating;
        }
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
        bool[][] newGrid = new bool[_gridManager.Width][];
        for (int index = 0; index < _gridManager.Width; index++)
        {
            newGrid[index] = new bool[_gridManager.Height];
        }

        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (_gridManager.IsWall(x, y))
                {
                    newGrid[x][y] = false;
                    continue;
                }
            
                RuleZone zone = _zoneManager.GetZoneForCell(x, y);
                int minSurvive = zone ? zone.minSurviveNeighbors : defaultRules.minSurviveNeighbors;
                int maxSurvive = zone ? zone.maxSurviveNeighbors : defaultRules.maxSurviveNeighbors;
                int reproduce = zone ? zone.reproduceNeighbors : defaultRules.reproduceNeighbors;

                int liveNeighbors = CountLiveNeighbors(x, y);
                bool isAlive = _gridManager.Grid[x, y];

                if (isAlive)
                {
                    newGrid[x][y] = liveNeighbors >= minSurvive && 
                                    liveNeighbors <= maxSurvive;
                }
                else
                {
                    newGrid[x][y] = liveNeighbors == reproduce;
                }
            }
        }

        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (!_gridManager.IsWall(x, y))
                {
                    _gridManager.SetCellState(x, y, newGrid[x][y]);
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

                if (!(0 <= checkX && checkX < _gridManager.Width && 0 <= checkY && checkY < _gridManager.Height))
                {
                    continue;
                }
                
                if (_gridManager.IsWall(checkX, checkY))
                    continue;

                if (_gridManager.Grid[checkX, checkY])
                {
                    ++count;
                }
            }
        }

        return count;
    }
}