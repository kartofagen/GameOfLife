using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LifeController : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int width = 50;
    [SerializeField] private int height = 50;
    [SerializeField] private float cellSize = 1.0f;
    
    [Header("Simulation Settings")]
    [SerializeField][Range(0.1f, 2.0f)] private float updateInterval = 0.5f;
    [SerializeField][Range(0, 100)] private int randomFillPercent = 50;
    
    [Header("Default Rules")]
    [SerializeField] private int minSurviveNeighbors = 2;
    [SerializeField] private int maxSurviveNeighbors = 3;
    [SerializeField] private int reproduceNeighbors = 3;

    [Header("Prefabs & Materials")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Material aliveMaterial;
    [SerializeField] private Material deadMaterial;
    [SerializeField] private Material zoneMaterial;

    [Header("Input Settings")]
    [SerializeField] private LayerMask gridLayerMask = 1;
    [SerializeField] private bool useRaycast = true;

    [Header("Custom Rules Zones")]
    [SerializeField] private List<RuleZone> ruleZones = new List<RuleZone>();

    private bool[,] grid;
    private GameObject[,] cellObjects;
    private bool isSimulating = false;
    private float timer = 0f;
    private Camera mainCamera;
    private Plane gridPlane;
    private RuleZone currentDrawingZone = null;
    private bool isDrawingZone = false;

    [System.Serializable]
    public class RuleZone
    {
        public string name = "New Zone";
        public int minSurviveNeighbors = 2;
        public int maxSurviveNeighbors = 3;
        public int reproduceNeighbors = 3;
        public Color zoneColor = new Color(1f, 0.5f, 0f, 0.3f);
        public bool[,] zoneCells;
        
        // Для визуализации
        [System.NonSerialized] public List<GameObject> zoneVisuals = new List<GameObject>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
        gridPlane = new Plane(Vector3.forward, Vector3.zero);
        InitializeGrid();
        CreateVisualGrid();
        RandomizeGrid();
        StartSimulation();
    }

    private void Update()
    {
        HandleInput();
        
        if (!isSimulating) return;

        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            ComputeNextGeneration();
            UpdateGridVisuals();
        }
    }

    private void HandleInput()
    {
        // Пауза/продолжение по пробелу
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleSimulation();
        }

        // Рисование зон по Z
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isDrawingZone)
                FinishDrawingZone();
            else
                StartDrawingZone();
        }

        // Рисование клеток мышью или зон
        if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            HandleMouseDrawing();
        }
    }

    private void StartDrawingZone()
    {
        isDrawingZone = true;
        currentDrawingZone = new RuleZone();
        currentDrawingZone.name = $"Zone {ruleZones.Count + 1}";
        currentDrawingZone.zoneCells = new bool[width, height];
        
        Debug.Log("Started drawing custom rule zone. Press Z again to finish.");
    }

    private void FinishDrawingZone()
    {
        if (!isDrawingZone || currentDrawingZone == null) return;
        
        // Добавляем зону только если есть хотя бы одна клетка
        bool hasCells = false;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
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
            ruleZones.Add(currentDrawingZone);
            UpdateZoneVisuals(currentDrawingZone);
            Debug.Log($"Finished drawing zone '{currentDrawingZone.name}' with {CountZoneCells(currentDrawingZone)} cells");
        }
        else
        {
            Debug.Log("Zone drawing canceled - no cells selected");
        }
        
        isDrawingZone = false;
        currentDrawingZone = null;
        
        // Восстанавливаем нормальные цвета всех клеток
        UpdateGridVisuals();
    }

    private int CountZoneCells(RuleZone zone)
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (zone.zoneCells[x, y]) count++;
            }
        }
        return count;
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
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int y = Mathf.RoundToInt(worldPos.y / cellSize);

        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (isDrawingZone)
            {
                // Рисование зоны - левая кнопка добавляет, правая убирает
                bool newState = Input.GetMouseButton(0);
                
                // Средняя кнопка мыши тоже добавляет для удобства
                if (Input.GetMouseButton(2))
                    newState = true;
                    
                if (currentDrawingZone.zoneCells[x, y] != newState)
                {
                    currentDrawingZone.zoneCells[x, y] = newState;
                    
                    // Визуальная обратная связь при рисовании зоны
                    UpdateTemporaryZoneVisual(x, y, newState);
                }
            }
            else
            {
                // Обычное рисование клеток - левая кнопка создает живые, правая - мертвые
                bool newState = Input.GetMouseButton(0);
                
                if (grid[x, y] != newState)
                {
                    grid[x, y] = newState;
                    UpdateCellVisual(x, y);
                }
            }
        }
    }

    private void UpdateTemporaryZoneVisual(int x, int y, bool state)
    {
        if (cellObjects[x, y] == null) return;

        Renderer renderer = cellObjects[x, y].GetComponent<Renderer>();
        if (renderer != null)
        {
            if (state)
            {
                // Временно подсвечиваем клетку цветом зоны
                Material tempMaterial = new Material(zoneMaterial);
                tempMaterial.color = new Color(1f, 0.5f, 0f, 0.7f);
                renderer.material = tempMaterial;
            }
            else
            {
                // Возвращаем обычный материал
                UpdateCellVisual(x, y);
            }
        }
    }

    private void UpdateZoneVisuals(RuleZone zone)
    {
        // Очищаем предыдущие визуализации
        foreach (GameObject visual in zone.zoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        zone.zoneVisuals.Clear();

        // Создаем визуализацию для зоны
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (zone.zoneCells[x, y])
                {
                    Vector3 position = new Vector3(x * cellSize, y * cellSize, -0.1f);
                    GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    zoneVisual.name = $"Zone_{zone.name}_{x}_{y}";
                    zoneVisual.transform.position = position;
                    zoneVisual.transform.localScale = Vector3.one * cellSize * 0.9f;
                    
                    // Убираем коллайдер, чтобы не мешал основным клеткам
                    Collider collider = zoneVisual.GetComponent<Collider>();
                    if (collider != null) Destroy(collider);
                    
                    Renderer renderer = zoneVisual.GetComponent<Renderer>();
                    Material zoneMat = new Material(zoneMaterial);
                    zoneMat.color = zone.zoneColor;
                    renderer.material = zoneMat;
                    
                    zoneVisual.transform.parent = transform;
                    zone.zoneVisuals.Add(zoneVisual);
                }
            }
        }
    }

    private void InitializeGrid()
    {
        grid = new bool[width, height];
        cellObjects = new GameObject[width, height];
    }

    private void CreateVisualGrid()
    {
        ClearVisualGrid();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
                GameObject cell = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";
                
                if (cell.GetComponent<Collider>() == null)
                {
                    BoxCollider collider = cell.AddComponent<BoxCollider>();
                    collider.size = new Vector3(cellSize, cellSize, 0.1f);
                }
                
                cellObjects[x, y] = cell;
                
                UpdateCellVisual(x, y);
            }
        }
        
        // Обновляем визуализацию всех зон
        foreach (RuleZone zone in ruleZones)
        {
            UpdateZoneVisuals(zone);
        }
    }

    private void ClearVisualGrid()
    {
        if (cellObjects == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (cellObjects[x, y] != null)
                {
                    DestroyImmediate(cellObjects[x, y]);
                }
            }
        }
        
        // Очищаем визуализацию зон
        foreach (RuleZone zone in ruleZones)
        {
            foreach (GameObject visual in zone.zoneVisuals)
            {
                if (visual != null) DestroyImmediate(visual);
            }
            zone.zoneVisuals.Clear();
        }
    }

    private void UpdateCellVisual(int x, int y)
    {
        if (cellObjects[x, y] == null) return;

        Renderer renderer = cellObjects[x, y].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = grid[x, y] ? aliveMaterial : deadMaterial;
        }
    }

    private void UpdateGridVisuals()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                UpdateCellVisual(x, y);
            }
        }
    }

    [ContextMenu("Randomize Grid")]
    public void RandomizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = Random.Range(0, 100) < randomFillPercent;
            }
        }
        UpdateGridVisuals();
    }

    [ContextMenu("Clear Grid")]
    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = false;
            }
        }
        UpdateGridVisuals();
    }

    [ContextMenu("Clear All Zones")]
    public void ClearAllZones()
    {
        foreach (RuleZone zone in ruleZones)
        {
            foreach (GameObject visual in zone.zoneVisuals)
            {
                if (visual != null) DestroyImmediate(visual);
            }
        }
        ruleZones.Clear();
    }

    public void StartSimulation()
    {
        isSimulating = true;
        Debug.Log("Simulation Started");
    }

    public void StopSimulation()
    {
        isSimulating = false;
        Debug.Log("Simulation Stopped");
    }

    [ContextMenu("Toggle Simulation")]
    public void ToggleSimulation()
    {
        isSimulating = !isSimulating;
        Debug.Log($"Simulation {(isSimulating ? "Resumed" : "Paused")}");
    }

    public void SetUpdateInterval(float interval)
    {
        updateInterval = Mathf.Clamp(interval, 0.1f, 2.0f);
    }

    private void ComputeNextGeneration()
    {
        bool[,] newGrid = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Определяем, какие правила применять к этой клетке
                RuleZone zone = GetZoneForCell(x, y);
                int minSurvive = zone != null ? zone.minSurviveNeighbors : minSurviveNeighbors;
                int maxSurvive = zone != null ? zone.maxSurviveNeighbors : maxSurviveNeighbors;
                int reproduce = zone != null ? zone.reproduceNeighbors : reproduceNeighbors;

                int liveNeighbors = CountLiveNeighbors(x, y);
                bool isAlive = grid[x, y];

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

        grid = newGrid;
    }

    private RuleZone GetZoneForCell(int x, int y)
    {
        foreach (RuleZone zone in ruleZones)
        {
            if (zone.zoneCells[x, y])
                return zone;
        }
        return null;
    }

    private int CountLiveNeighbors(int x, int y)
    {
        int count = 0;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                int checkX = (x + i + width) % width;
                int checkY = (y + j + height) % height;

                if (grid[checkX, checkY])
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
        InitializeGrid();
        CreateVisualGrid();
    }

    private void OnDestroy()
    {
        ClearVisualGrid();
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        // Отображение позиции мыши
        Vector3 mousePos = Input.mousePosition;
        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        float distance;
        
        if (gridPlane.Raycast(ray, out distance))
        {
            Vector3 worldPos = ray.GetPoint(distance);
            int x = Mathf.RoundToInt(worldPos.x / cellSize);
            int y = Mathf.RoundToInt(worldPos.y / cellSize);
            
            Gizmos.color = isDrawingZone ? Color.yellow : Color.red;
            Gizmos.DrawWireCube(new Vector3(x * cellSize, y * cellSize, 0), 
                               new Vector3(cellSize, cellSize, cellSize));
        }
        
        // Отображение текущей рисуемой зоны
        if (isDrawingZone && currentDrawingZone != null)
        {
            Gizmos.color = currentDrawingZone.zoneColor;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (currentDrawingZone.zoneCells[x, y])
                    {
                        Vector3 pos = new Vector3(x * cellSize, y * cellSize, -0.05f);
                        Gizmos.DrawCube(pos, Vector3.one * cellSize * 0.8f);
                    }
                }
            }
        }
    }
}