using UnityEngine;
using System.Collections.Generic;

public class RuleZoneManager : MonoBehaviour
{
    [Header("Zone Materials")]
    [SerializeField] private Material zoneMaterial;

    [Header("Custom Rules Zones")]
    [SerializeField] private List<RuleZone> ruleZones = new List<RuleZone>();

    private GridManager _gridManager;
    private List<GameObject> _temporaryZoneVisuals = new List<GameObject>();

    public void Initialize(GridManager gridManager)
    {
        _gridManager = gridManager;
        
        foreach (var zone in ruleZones)
        {
            if (zone.ZoneCells == null || zone.ZoneCells.Length == 0)
            {
                zone.ZoneCells = new bool[gridManager.Width, gridManager.Height];
            }
        }
    }

    private void UpdateZoneVisuals(RuleZone zone)
    {
        if (zone == null || _gridManager == null) return;

        foreach (GameObject visual in zone.ZoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        zone.ZoneVisuals.Clear();

        for (int x = 0; x < _gridManager.Width; ++x)
        {
            for (int y = 0; y < _gridManager.Height; ++y)
            {
                if (zone.ZoneCells[x, y])
                {
                    CreateZoneVisual(zone, x, y, -0.1f, zone.zoneColor, false);
                }
            }
        }
    }

    public RuleZone GetZoneForCell(int x, int y)
    {
        for (int i = ruleZones.Count - 1; i >= 0; --i)
        {
            RuleZone zone = ruleZones[i];
            if (zone.ZoneCells != null && x >= 0 && x < zone.ZoneCells.GetLength(0) && 
                y >= 0 && y < zone.ZoneCells.GetLength(1) && zone.ZoneCells[x, y])
                return zone;
        }
        return null;
    }

    private void AddZone(RuleZone zone)
    {
        if (zone == null) return;

        if (zone.ZoneCells == null || zone.ZoneCells.Length == 0)
        {
            zone.ZoneCells = new bool[_gridManager.Width, _gridManager.Height];
        }

        ruleZones.Add(zone);
        UpdateZoneVisuals(zone);
    }

    public void ClearAllZones()
    {
        foreach (RuleZone zone in ruleZones)
        {
            foreach (GameObject visual in zone.ZoneVisuals)
            {
                if (visual != null) Destroy(visual);
            }
            zone.ZoneVisuals.Clear();
        }
        ruleZones.Clear();
    }

    public void UpdateZoneFromStructure(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        if (zoneStructure == null || _gridManager == null) return;

        RuleZone zone = ScriptableObject.CreateInstance<RuleZone>();
        zone.zoneName = zoneStructure.structureName;
        zone.minSurviveNeighbors = zoneStructure.minSurviveNeighbors;
        zone.maxSurviveNeighbors = zoneStructure.maxSurviveNeighbors;
        zone.reproduceNeighbors = zoneStructure.reproduceNeighbors;
        zone.zoneColor = zoneStructure.zoneColor;
        zone.ZoneCells = new bool[_gridManager.Width, _gridManager.Height];

        zoneStructure.InitializeCells();
        if (zoneStructure.Cells == null) return;

        int structureWidth = zoneStructure.Cells.GetLength(0);
        int structureHeight = zoneStructure.Cells.GetLength(1);

        for (int x = 0; x < structureWidth; ++x)
        {
            for (int y = 0; y < structureHeight; ++y)
            {
                if (zoneStructure.Cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < _gridManager.Width && 
                        worldY >= 0 && worldY < _gridManager.Height &&
                        !_gridManager.IsWall(worldX, worldY))
                    {
                        zone.ZoneCells[worldX, worldY] = true;
                    }
                }
            }
        }

        AddZone(zone);
    }

    public void UpdateTemporaryZoneVisual(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        ClearTemporaryZoneVisual();

        if (zoneStructure == null || _gridManager == null) return;

        zoneStructure.InitializeCells();
        if (zoneStructure.Cells == null) return;

        int structureWidth = zoneStructure.Cells.GetLength(0);
        int structureHeight = zoneStructure.Cells.GetLength(1);

        for (int x = 0; x < structureWidth; x++)
        {
            for (int y = 0; y < structureHeight; y++)
            {
                if (zoneStructure.Cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < _gridManager.Width && 
                        worldY >= 0 && worldY < _gridManager.Height &&
                        !_gridManager.IsWall(worldX, worldY))
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
        Vector3 position = new Vector3(x * _gridManager.CellSize, y * _gridManager.CellSize, zDepth) + transform.position;
        GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        zoneVisual.name = isTemporary ? $"TempZone_{x}_{y}" : $"Zone_{zone.zoneName}_{x}_{y}";
        zoneVisual.transform.position = position;
        zoneVisual.transform.localScale = Vector3.one * _gridManager.CellSize * 0.9f;

        Collider zoneCollider = zoneVisual.GetComponent<Collider>();
        if (zoneCollider != null) Destroy(zoneCollider);

        Renderer zoneRenderer = zoneVisual.GetComponent<Renderer>();
        Material zoneMat = new Material(zoneMaterial)
        {
            color = color
        };
        zoneRenderer.material = zoneMat;

        zoneVisual.transform.parent = transform;

        if (isTemporary)
        {
            _temporaryZoneVisuals.Add(zoneVisual);
        }
        else if (zone != null)
        {
            zone.ZoneVisuals.Add(zoneVisual);
        }
    }

    public void ClearTemporaryZoneVisual()
    {
        foreach (GameObject visual in _temporaryZoneVisuals)
        {
            if (visual != null) Destroy(visual);
        }
        _temporaryZoneVisuals.Clear();
    }

    public bool IsZonePlacementValid(ZoneStructureData zoneStructure, Vector2Int gridPosition)
    {
        if (zoneStructure == null || _gridManager == null) return false;

        zoneStructure.InitializeCells();
        if (zoneStructure.Cells == null) return false;

        int structureWidth = zoneStructure.Cells.GetLength(0);
        int structureHeight = zoneStructure.Cells.GetLength(1);
        bool atLeastOneCellInside = false;

        for (int x = 0; x < structureWidth; ++x)
        {
            for (int y = 0; y < structureHeight; ++y)
            {
                if (zoneStructure.Cells[x, y])
                {
                    int worldX = gridPosition.x + x - structureWidth / 2;
                    int worldY = gridPosition.y + y - structureHeight / 2;

                    if (worldX >= 0 && worldX < _gridManager.Width && 
                        worldY >= 0 && worldY < _gridManager.Height)
                    {
                        atLeastOneCellInside = true;
                    }
                }
            }
        }
    
        return atLeastOneCellInside;
    }
    
    public bool RemoveZoneAtPosition(Vector2Int gridPosition)
    {
        for (int i = ruleZones.Count - 1; i >= 0; --i)
        {
            RuleZone zone = ruleZones[i];
            if (zone.ZoneCells != null && 
                gridPosition.x >= 0 && gridPosition.x < zone.ZoneCells.GetLength(0) &&
                gridPosition.y >= 0 && gridPosition.y < zone.ZoneCells.GetLength(1) &&
                zone.ZoneCells[gridPosition.x, gridPosition.y])
            {
                foreach (GameObject visual in zone.ZoneVisuals)
                {
                    if (visual != null) Destroy(visual);
                }
                zone.ZoneVisuals.Clear();
            
                ruleZones.RemoveAt(i);
                
                return true;
            }
        }
        return false;
    }
}