using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RuleZone
{
    public string name = "New Zone";
    public int minSurviveNeighbors = 2;
    public int maxSurviveNeighbors = 3;
    public int reproduceNeighbors = 3;
    public Color zoneColor = new Color(1f, 0f, 0f, 0.3f);
    public bool[,] zoneCells;
    
    [System.NonSerialized] public List<GameObject> zoneVisuals = new List<GameObject>();
}
