using UnityEngine;

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

    private bool[,] grid;
    private GameObject[,] cellObjects;
    private bool[,] walls;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public bool[,] Grid => grid;

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
}
