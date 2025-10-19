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
    [SerializeField] private Color aliveColor;
    [SerializeField] private Color deadColor;
    
    [Header("Walls Settings")]
    [SerializeField] private GameObject wallsSource;

    [Header("Structures")]
    [SerializeField] private List<StructureData> availableStructures = new List<StructureData>();
    [SerializeField] private Material structurePreviewMaterial;

    private bool[,] grid;
    private GameObject[,] cellObjects;
    private bool[,] walls;

    // Structure placement
    private StructureData currentStructure = null;
    private bool isPlacingStructure = false;
    private int structureRotation = 0;
    private List<GameObject> structurePreview = new List<GameObject>();
    private bool[,] currentStructureCells;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public bool[,] Grid => grid;
    public List<StructureData> AvailableStructures => availableStructures;

    public void InitializeGrid()
    {
        grid = new bool[width, height];
        walls = new bool[width, height];
        cellObjects = new GameObject[width, height];
        
        LoadWalls();
    }
    
    private void LoadWalls()
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                walls[x, y] = false;
            }
        }

        foreach (Transform child in wallsSource.transform)
        {
            string name = child.gameObject.name;
            
            if (name.StartsWith("cell_"))
            {
                string[] parts = name.Split('_');
                if (parts.Length == 3 && 
                    int.TryParse(parts[1], out int x) && 
                    int.TryParse(parts[2], out int y))
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        walls[x, y] = true;
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
                if (walls[x, y])
                {
                    continue;
                }

                Vector3 position = transform.position + new Vector3(x * cellSize, y * cellSize, 0);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"cell_{x}_{y}";

                if (cell.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = cell.AddComponent<BoxCollider>();
                    collider.size = new Vector3(cellSize, cellSize, 0.1f);
                }

                cellObjects[x, y] = cell;
                UpdateCellVisual(x, y);
            }
        }
    }

    private void ClearVisualGrid()
    {
        if (cellObjects == null) return;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (cellObjects[x, y] != null)
                {
                    Destroy(cellObjects[x, y]);
                }
            }
        }
    }

    public void UpdateCellVisual(int x, int y)
    {
        if (cellObjects[x, y] == null) return;

        Renderer renderer = cellObjects[x, y].GetComponent<Renderer>();
        if (renderer != null)
        {
            if (!walls[x, y])
            {
                renderer.material.color = grid[x, y] ? aliveColor : deadColor;
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
            grid[x, y] = state;
            UpdateCellVisual(x, y);
        }
    }

    public bool GetCellState(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return grid[x, y];
        return false;
    }

    public bool IsWall(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
            return walls[x, y];
        return false;
    }

    public void RandomizeGrid(float fillShare)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                if (walls[x, y])
                    continue;
                
                grid[x, y] = Random.Range(0, 1f) < fillShare;
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
        return cellObjects;
    }
    
    public void StartStructurePlacement(StructureData structure)
    {
        if (isPlacingStructure)
            CancelStructurePlacement();

        if (structure == null)
        {
            Debug.LogError("Cannot place null structure!");
            return;
        }

        // Инициализируем клетки структуры
        structure.InitializeCells();
        
        if (structure.cells == null)
        {
            Debug.LogError($"Structure {structure.structureName} has null cells array!");
            return;
        }

        currentStructure = structure;
        currentStructureCells = structure.cells; // Кэшируем
        isPlacingStructure = true;
        structureRotation = 0;
        
        Debug.Log($"Placing structure: {structure.structureName} ({structure.width}x{structure.height})");
    }

    private bool[,] GetRotatedStructureCells()
    {
        if (currentStructure == null || currentStructureCells == null) 
            return new bool[0,0];

        bool[,] original = currentStructureCells;
        int width = original.GetLength(0);
        int height = original.GetLength(1);

        return structureRotation switch
        {
            0 => original, // 0°
            1 => RotateMatrix(original, true), // 90° clockwise
            2 => RotateMatrix(RotateMatrix(original, true), true), // 180°
            3 => RotateMatrix(original, false), // 90° counter-clockwise
            _ => original
        };
    }

    private bool[,] RotateMatrix(bool[,] matrix, bool clockwise)
    {
        int width = matrix.GetLength(0);
        int height = matrix.GetLength(1);
        bool[,] rotated = new bool[height, width];
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (clockwise)
                    rotated[y, width - 1 - x] = matrix[x, y];
                else
                    rotated[height - 1 - y, x] = matrix[x, y];
            }
        }
        
        return rotated;
    }

    public void UpdateStructurePreview(Vector2Int gridPosition)
    {
        ClearStructurePreview();

        if (!isPlacingStructure || currentStructure == null || currentStructureCells == null) return;

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

                    // Check if position is valid
                    if (worldX >= 0 && worldX < width && worldY >= 0 && worldY < height && !walls[worldX, worldY])
                    {
                        Vector3 position = transform.position + new Vector3(worldX * cellSize, worldY * cellSize, -0.1f);
                        GameObject previewCell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                        previewCell.name = "StructurePreview";
                        previewCell.transform.position = position;
                        previewCell.transform.localScale = Vector3.one * cellSize * 0.8f;

                        // Remove collider
                        Collider collider = previewCell.GetComponent<Collider>();
                        if (collider != null) Destroy(collider);

                        // Set preview material
                        Renderer renderer = previewCell.GetComponent<Renderer>();
                        Material previewMat = new Material(structurePreviewMaterial);
                        
                        // Use different color based on validity
                        bool isValid = IsStructurePlacementValid(gridPosition);
                        Color previewColor = isValid ? 
                            new Color(0.2f, 0.8f, 0.2f, 0.6f) : // Green if valid
                            new Color(0.8f, 0.2f, 0.2f, 0.6f);  // Red if invalid
                        
                        previewMat.color = previewColor;
                        renderer.material = previewMat;

                        previewCell.transform.parent = transform;
                        structurePreview.Add(previewCell);
                    }
                }
            }
        }
    }

    private bool IsStructurePlacementValid(Vector2Int gridPosition)
    {
        if (currentStructure == null || currentStructureCells == null) return false;

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

                    if (worldX < 0 || worldX >= width || worldY < 0 || worldY >= height || walls[worldX, worldY])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    public bool PlaceStructure(Vector2Int gridPosition)
    {
        if (!isPlacingStructure || currentStructure == null || currentStructureCells == null) 
        {
            Debug.LogError("Cannot place structure - invalid state");
            return false;
        }

        if (!IsStructurePlacementValid(gridPosition))
        {
            Debug.LogWarning("Cannot place structure here - out of bounds or on wall");
            return false;
        }

        bool[,] rotatedCells = GetRotatedStructureCells();
        int structureWidth = rotatedCells.GetLength(0);
        int structureHeight = rotatedCells.GetLength(1);

        // Place the structure
        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (rotatedCells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;
                    SetCellState(worldX, worldY, true);
                }
            }
        }

        Debug.Log($"Structure '{currentStructure.structureName}' placed at ({gridPosition.x}, {gridPosition.y})");
        CancelStructurePlacement();
        return true;
    }
    
    private void ClearStructurePreview()
    {
        foreach (GameObject preview in structurePreview)
        {
            if (preview != null) Destroy(preview);
        }
        structurePreview.Clear();
    }
    
    public void RotateStructure()
    {
        if (!isPlacingStructure || currentStructure == null) return;
        
        structureRotation = (structureRotation + 1) % 4;
        UpdateStructurePreview(Vector2Int.zero);
        Debug.Log($"Structure rotated to {structureRotation * 90}°");
    }

    public void CancelStructurePlacement()
    {
        currentStructure = null;
        currentStructureCells = null;
        isPlacingStructure = false;
        ClearStructurePreview();
        Debug.Log("Structure placement canceled");
    }

    public bool IsPlacingStructure => isPlacingStructure;
}
