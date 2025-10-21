using UnityEngine;
using System.Collections.Generic;

public class RuleZoneManager : MonoBehaviour
{
    [Header("Zone Materials")]
    [SerializeField] private Material zoneMaterial;

    [Header("Custom Rules Zones")]
    [SerializeField] private List<RuleZone> ruleZones = new List<RuleZone>();

    private GridManager gridManager;
    private List<GameObject> temporaryZoneVisuals = new List<GameObject>();

    public List<RuleZone> RuleZones => ruleZones;

    public void Initialize(GridManager gridManager)
    {
        this.gridManager = gridManager;
        
        foreach (var zone in ruleZones)
        {
            if (zone.zoneCells == null || zone.zoneCells.Length == 0)
            {
                zone.zoneCells = new bool[gridManager.Width, gridManager.Height];
            }
        }
    }

    public void UpdateZoneVisuals(RuleZone zone)
    {
        if (zone == null || gridManager == null) return;

        foreach (GameObject visual in zone.zoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        zone.zoneVisuals.Clear();

        for (int x = 0; x < gridManager.Width; ++x)
        {
            for (int y = 0; y < gridManager.Height; ++y)
            {
                if (zone.zoneCells[x, y])
                {
                    CreateZoneVisual(zone, x, y, -0.1f, zone.zoneColor, false);
                }
            }
        }
    }

    public RuleZone GetZoneForCell(int x, int y)
    {
        foreach (RuleZone zone in ruleZones)
        {
            if (zone.zoneCells != null && x >= 0 && x < zone.zoneCells.GetLength(0) && 
                y >= 0 && y < zone.zoneCells.GetLength(1) && zone.zoneCells[x, y])
                return zone;
        }
        return null;
    }

    public void AddZone(RuleZone zone)
    {
        if (zone == null) return;

        // Initialize zone cells if needed
        if (zone.zoneCells == null || zone.zoneCells.Length == 0)
        {
            zone.zoneCells = new bool[gridManager.Width, gridManager.Height];
        }

        ruleZones.Add(zone);
        UpdateZoneVisuals(zone);
    }

    public void RemoveZone(RuleZone zone)
    {
        if (zone == null) return;

        // Clear visuals
        foreach (GameObject visual in zone.zoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        zone.zoneVisuals.Clear();

        ruleZones.Remove(zone);
    }

    public void ClearAllZones()
    {
        foreach (RuleZone zone in ruleZones)
        {
            foreach (GameObject visual in zone.zoneVisuals)
            {
                if (visual != null) Destroy(visual);
            }
            zone.zoneVisuals.Clear();
        }
        ruleZones.Clear();
    }

    public void UpdateZoneFromStructure(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        if (zoneStructure == null || gridManager == null) return;

        // Create or find existing zone
        RuleZone zone = ScriptableObject.CreateInstance<RuleZone>();
        zone.zoneName = zoneStructure.structureName;
        zone.minSurviveNeighbors = zoneStructure.minSurviveNeighbors;
        zone.maxSurviveNeighbors = zoneStructure.maxSurviveNeighbors;
        zone.reproduceNeighbors = zoneStructure.reproduceNeighbors;
        zone.zoneColor = zoneStructure.zoneColor;
        zone.zoneCells = new bool[gridManager.Width, gridManager.Height];

        // Initialize structure cells
        zoneStructure.InitializeCells();
        if (zoneStructure.cells == null) return;

        int structureWidth = zoneStructure.cells.GetLength(0);
        int structureHeight = zoneStructure.cells.GetLength(1);

        // Apply structure pattern to zone
        for (int x = 0; x < structureWidth; ++x)
        {
            for (int y = 0; y < structureHeight; ++y)
            {
                if (zoneStructure.cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < gridManager.Width && 
                        worldY >= 0 && worldY < gridManager.Height)
                    {
                        zone.zoneCells[worldX, worldY] = true;
                    }
                }
            }
        }

        AddZone(zone);
    }

    public void UpdateTemporaryZoneVisual(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        ClearTemporaryZoneVisual();

        if (zoneStructure == null || gridManager == null) return;

        zoneStructure.InitializeCells();
        if (zoneStructure.cells == null) return;

        int structureWidth = zoneStructure.cells.GetLength(0);
        int structureHeight = zoneStructure.cells.GetLength(1);

        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (zoneStructure.cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < gridManager.Width && 
                        worldY >= 0 && worldY < gridManager.Height)
                    {
                        CreateZoneVisual(null, worldX, worldY, -0.2f, 
                            new Color(zoneStructure.zoneColor.r, zoneStructure.zoneColor.g, 
                                      zoneStructure.zoneColor.b, zoneStructure.zoneColor.a + 0.3f), true);
                    }
                }
            }
        }
    }

    private void CreateZoneVisual(RuleZone zone, int x, int y, float zDepth, Color color, bool isTemporary)
    {
        Vector3 position = new Vector3(x * gridManager.CellSize, y * gridManager.CellSize, zDepth) + transform.position;
        GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        zoneVisual.name = isTemporary ? $"TempZone_{x}_{y}" : $"Zone_{zone.zoneName}_{x}_{y}";
        zoneVisual.transform.position = position;
        zoneVisual.transform.localScale = Vector3.one * gridManager.CellSize * 0.9f;

        Collider collider = zoneVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);

        Renderer renderer = zoneVisual.GetComponent<Renderer>();
        Material zoneMat = new Material(zoneMaterial);
        zoneMat.color = color;
        renderer.material = zoneMat;

        zoneVisual.transform.parent = transform;

        if (isTemporary)
        {
            temporaryZoneVisuals.Add(zoneVisual);
        }
        else if (zone != null)
        {
            zone.zoneVisuals.Add(zoneVisual);
        }
    }

    public void ClearTemporaryZoneVisual()
    {
        foreach (GameObject visual in temporaryZoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        temporaryZoneVisuals.Clear();
    }

    public bool IsZonePlacementValid(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        if (zoneStructure == null || gridManager == null) return false;

        zoneStructure.InitializeCells();
        if (zoneStructure.cells == null) return false;

        int structureWidth = zoneStructure.cells.GetLength(0);
        int structureHeight = zoneStructure.cells.GetLength(1);

        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (zoneStructure.cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    // Check bounds
                    if (worldX < 0 || worldX >= gridManager.Width || 
                        worldY < 0 || worldY >= gridManager.Height)
                    {
                        return false;
                    }

                    // Check if overlaps with existing zone
                    if (GetZoneForCell(worldX, worldY) != null)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }
}