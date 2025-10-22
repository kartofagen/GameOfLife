using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 64;
    [SerializeField] private int height = 64;
    [SerializeField] private float cellSize = 1.0f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject cellPrefab;
    
    [Header("Walls Settings")]
    [SerializeField] private GameObject wallsSource;

    [Header("Structures")]
    [SerializeField] private List<StructureData> availableStructures;
    [SerializeField] private Material structurePreviewMaterial;
    
    [Header("Zone Structures")]
    [SerializeField] private List<ZoneStructureData> availableZoneStructures;
    [SerializeField] private Material zoneStructurePreviewMaterial;

    private bool[,] _grid;
    private GameObject[,] _cellObjects;
    private bool[,] _walls;

    private StructureData _currentStructure = null;
    private bool _isPlacingStructure = false;
    private int _structureRotation = 0;
    private List<GameObject> _structurePreview = new List<GameObject>();
    private bool[,] _currentStructureCells;

    private ZoneStructureData _currentZoneStructure = null;
    private bool _isPlacingZoneStructure = false;
    private List<GameObject> _zoneStructurePreview = new List<GameObject>();
    private bool[,] _currentZoneStructureCells;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public bool[,] Grid => _grid;
    public List<StructureData> AvailableStructures => availableStructures;

    public bool IsPlacingZoneStructure => _isPlacingZoneStructure;
    public List<ZoneStructureData> AvailableZoneStructures => availableZoneStructures;

    public void InitializeGrid()
    {
        _grid = new bool[width, height];
        _walls = new bool[width, height];
        _cellObjects = new GameObject[width, height];
        
        LoadWalls();
    }
    
    private void LoadWalls()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                _walls[x, y] = false;
            }
        }

        foreach (Transform child in wallsSource.transform)
        {
            string childName = child.gameObject.name;
            
            if (childName.StartsWith("cell_"))
            {
                string[] parts = childName.Split('_');
                if (parts.Length == 3 && 
                    int.TryParse(parts[1], out int x) && 
                    int.TryParse(parts[2], out int y))
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        _walls[x, y] = true;
                    }
                }
            }
        }
    }

    public void CreateVisualGrid()
    {
        ClearVisualGrid();

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (_walls[x, y])
                {
                    continue;
                }

                Vector3 position = transform.position + new Vector3(x * cellSize, y * cellSize, 0);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"cell_{x}_{y}";

                if (cell.GetComponent<Collider>() == null)
                {
                    BoxCollider boxCollider = cell.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(cellSize, cellSize, 0.1f);
                }

                _cellObjects[x, y] = cell;
                UpdateCellVisual(x, y);
            }
        }
    }

    private void ClearVisualGrid()
    {
        if (_cellObjects == null) return;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (_cellObjects[x, y] != null)
                {
                    Destroy(_cellObjects[x, y]);
                }
            }
        }
    }

    public void UpdateCellVisual(int x, int y)
    {
        if (!_cellObjects[x, y]) return;

        LifeAnimation manager = _cellObjects[x, y].GetComponent<LifeAnimation>();
        if (manager)
        {
            if (!_walls[x, y])
            {
                manager.SetAlive(_grid[x, y]);
            }
        }
    }

    public void UpdateGridVisuals()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                UpdateCellVisual(x, y);
            }
        }
    }

    public void SetCellState(int x, int y, bool state)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            _grid[x, y] = state;
            UpdateCellVisual(x, y);
        }
    }

    public bool GetCellState(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return _grid[x, y];
        return false;
    }

    public bool IsWall(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return _walls[x, y];
        return false;
    }

    public void RandomizeGrid(float fillShare)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (_walls[x, y])
                    continue;
                
                _grid[x, y] = Random.Range(0, 1f) < fillShare;
            }
        }
        UpdateGridVisuals();
    }

    private void OnDestroy()
    {
        ClearVisualGrid();
    }
    
    public GameObject[,] GetCellObjects()
    {
        return _cellObjects;
    }
    
    public void StartStructurePlacement(StructureData structure)
    {
        if (_isPlacingStructure)
            CancelStructurePlacement();

        if (structure == null)
        {
            return;
        }

        structure.InitializeCells();
        
        if (structure.Cells == null)
        {
            return;
        }

        _currentStructure = structure;
        _currentStructureCells = structure.Cells;
        _isPlacingStructure = true;
        _structureRotation = 0;
    }

    public bool[,] GetRotatedStructureCells()
    {
        if (_currentStructure == null || _currentStructureCells == null) 
            return new bool[0,0];

        bool[,] original = _currentStructureCells;

        return _structureRotation switch
        {
            0 => original,
            1 => RotateMatrix(original, true),
            2 => RotateMatrix(RotateMatrix(original, true), true),
            3 => RotateMatrix(original, false),
            _ => original
        };
    }

    private bool[,] RotateMatrix(bool[,] matrix, bool clockwise)
    {
        int matrixWidth = matrix.GetLength(0);
        int matrixHeight = matrix.GetLength(1);
        bool[,] rotated = new bool[matrixHeight, matrixWidth];
        
        for (int x = 0; x < matrixWidth; ++x)
        {
            for (int y = 0; y < matrixHeight; ++y)
            {
                if (clockwise)
                    rotated[y, matrixWidth - 1 - x] = matrix[x, y];
                else
                    rotated[matrixHeight - 1 - y, x] = matrix[x, y];
            }
        }
        
        return rotated;
    }

    public void UpdateStructurePreview(Vector2Int gridPosition)
    {
        ClearStructurePreview();

        if (!_isPlacingStructure || _currentStructure == null || _currentStructureCells == null) return;

        bool[,] rotatedCells = GetRotatedStructureCells();
        int structureWidth = rotatedCells.GetLength(0);
        int structureHeight = rotatedCells.GetLength(1);

        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (rotatedCells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < width && worldY >= 0 && worldY < height && !_walls[worldX, worldY])
                    {
                        Vector3 position = transform.position + new Vector3(worldX * cellSize, worldY * cellSize, -0.1f);
                        GameObject previewCell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        previewCell.name = "StructurePreview";
                        previewCell.transform.position = position;
                        previewCell.transform.localScale = Vector3.one * cellSize * 0.8f;

                        Collider previewCollider = previewCell.GetComponent<Collider>();
                        if (previewCollider != null) Destroy(previewCollider);

                        Renderer previewRenderer = previewCell.GetComponent<Renderer>();
                        Material previewMat = new Material(structurePreviewMaterial);
                        
                        bool isValid = IsStructurePlacementValid(gridPosition);
                        Color previewColor = isValid ? 
                            new Color(0.2f, 0.8f, 0.2f, 0.6f) :
                            new Color(0.8f, 0.2f, 0.2f, 0.6f);
                        
                        previewMat.color = previewColor;
                        previewRenderer.material = previewMat;

                        previewCell.transform.parent = transform;
                        _structurePreview.Add(previewCell);
                    }
                }
            }
        }
    }

    private bool IsStructurePlacementValid(Vector2Int gridPosition)
    {
        if (_currentStructure == null || _currentStructureCells == null) return false;

        bool[,] rotatedCells = GetRotatedStructureCells();
        int structureWidth = rotatedCells.GetLength(0);
        int structureHeight = rotatedCells.GetLength(1);

        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (rotatedCells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX < 0 || worldX >= width || worldY < 0 || worldY >= height || _walls[worldX, worldY])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public int PlaceStructure(Vector2Int gridPosition)
    {
        if (!_isPlacingStructure || _currentStructure == null || _currentStructureCells == null) 
        {
            return 0;
        }

        if (!IsStructurePlacementValid(gridPosition))
        {
            return 0;
        }

        bool[,] rotatedCells = GetRotatedStructureCells();
        int structureWidth = rotatedCells.GetLength(0);
        int structureHeight = rotatedCells.GetLength(1);
        int cellsAdded = 0;

        for (int x = 0; x < structureWidth; ++x)
        {
            for (int y = 0; y < structureHeight; ++y)
            {
                if (rotatedCells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;
                
                    if (!GetCellState(worldX, worldY))
                    {
                        SetCellState(worldX, worldY, true);
                        ++cellsAdded;
                    }
                }
            }
        }

        return cellsAdded;
    }

    private void ClearStructurePreview()
    {
        foreach (GameObject preview in _structurePreview)
        {
            if (preview != null) Destroy(preview);
        }
        _structurePreview.Clear();
    }
    
    public void RotateStructure()
    {
        if (!_isPlacingStructure || _currentStructure == null) return;
        
        _structureRotation = (_structureRotation + 1) % 4;
        UpdateStructurePreview(Vector2Int.zero);
    }

    public void CancelStructurePlacement()
    {
        _currentStructure = null;
        _currentStructureCells = null;
        _isPlacingStructure = false;
        ClearStructurePreview();
    }

    public bool IsPlacingStructure => _isPlacingStructure;
    
    public void StartZoneStructurePlacement(ZoneStructureData zoneStructure)
    {
        if (_isPlacingZoneStructure)
            CancelZoneStructurePlacement();

        if (zoneStructure == null)
        {
            return;
        }

        zoneStructure.InitializeCells();
        
        if (zoneStructure.Cells == null)
        {
            return;
        }

        _currentZoneStructure = zoneStructure;
        _currentZoneStructureCells = zoneStructure.Cells;
        _isPlacingZoneStructure = true;
    }

    public void UpdateZoneStructurePreview(Vector2Int gridPosition)
    {
        ClearZoneStructurePreview();

        if (!_isPlacingZoneStructure || _currentZoneStructure == null || _currentZoneStructureCells == null) return;

        RuleZoneManager zoneManager = GetComponent<RuleZoneManager>();
        if (zoneManager != null)
        {
            zoneManager.UpdateTemporaryZoneVisual(_currentZoneStructure, gridPosition);
        }
    }

    public bool PlaceZoneStructure(Vector2Int gridPosition)
    {
        TournamentManager tournamentManager = GetComponent<TournamentManager>();
        if (tournamentManager != null && tournamentManager.IsTournament && 
            !tournamentManager.CanPlaceZone())
        {
            return false;
        }
        
        if (!_isPlacingZoneStructure || _currentZoneStructure == null || _currentZoneStructureCells == null) 
        {
            return false;
        }

        RuleZoneManager zoneManager = GetComponent<RuleZoneManager>();
        if (zoneManager == null || !zoneManager.IsZonePlacementValid(_currentZoneStructure, gridPosition))
        {
            return false;
        }

        zoneManager.UpdateZoneFromStructure(_currentZoneStructure, gridPosition);
        return true;
    }

    private void ClearZoneStructurePreview()
    {
        foreach (GameObject preview in _zoneStructurePreview)
        {
            if (preview != null) Destroy(preview);
        }
        _zoneStructurePreview.Clear();
    }

    public void CancelZoneStructurePlacement()
    {
        RuleZoneManager zoneManager = GetComponent<RuleZoneManager>();
        if (zoneManager != null)
        {
            zoneManager.ClearTemporaryZoneVisual();
        }

        _currentZoneStructure = null;
        _currentZoneStructureCells = null;
        _isPlacingZoneStructure = false;
        ClearZoneStructurePreview();
    }
}