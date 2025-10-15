using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float cellSize = 1.0f;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Material aliveMaterial;
    [SerializeField] private Material deadMaterial;

    private bool[,] grid;
    private GameObject[,] cellObjects;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public bool[,] Grid => grid;

    public void InitializeGrid()
    {
        grid = new bool[width, height];
        cellObjects = new GameObject[width, height];
    }

    public void CreateVisualGrid()
    {
        ClearVisualGrid();

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
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
                    DestroyImmediate(cellObjects[x, y]);
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
            renderer.material = grid[x, y] ? aliveMaterial : deadMaterial;
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

    public void RandomizeGrid(float fillShare)
    {
        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                grid[x, y] = Random.Range(0, 1f) < fillShare;
            }
        }
        UpdateGridVisuals();
    }

    public void RecreateGrid()
    {
        InitializeGrid();
        CreateVisualGrid();
    }

    private void OnDestroy()
    {
        ClearVisualGrid();
    }
}
