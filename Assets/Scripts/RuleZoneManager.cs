using UnityEngine;
using System.Collections.Generic;

public class RuleZoneManager : MonoBehaviour
{
    [Header("Zone Materials")]
    [SerializeField] private Material zoneMaterial;

    [Header("Custom Rules Zones")]
    [SerializeField] private List<RuleZone> ruleZones = new List<RuleZone>();

    private GridManager gridManager;

    public List<RuleZone> RuleZones => ruleZones;

    public void Initialize(GridManager gridManager)
    {
        this.gridManager = gridManager;
    }

    public void UpdateZoneVisuals(RuleZone zone)
    {
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
                    Vector3 position = new Vector3(x * gridManager.CellSize, y * gridManager.CellSize, -0.1f);
                    GameObject zoneVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    zoneVisual.name = $"Zone_{zone.name}_{x}_{y}";
                    zoneVisual.transform.position = position;
                    zoneVisual.transform.localScale = Vector3.one * gridManager.CellSize * 0.9f;

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

    public RuleZone GetZoneForCell(int x, int y)
    {
        foreach (RuleZone zone in ruleZones)
        {
            if (zone.zoneCells[x, y])
                return zone;
        }
        return null;
    }

    public void AddZone(RuleZone zone)
    {
        ruleZones.Add(zone);
        UpdateZoneVisuals(zone);
    }
}
