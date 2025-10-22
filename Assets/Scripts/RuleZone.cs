using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rules", menuName = "Game of Life/Rules")]
public class RuleZone : ScriptableObject
{
    public string zoneName = "New Zone";
    [Range(0, 8)] public int minSurviveNeighbors = 2;
    [Range(0, 8)] public int maxSurviveNeighbors = 3;
    [Range(0, 8)] public int reproduceNeighbors = 3;
    public Color zoneColor = new Color(1f, 0f, 0f, 0.3f);
    public bool[,] ZoneCells;
    
    [System.NonSerialized] public List<GameObject> ZoneVisuals = new List<GameObject>();
}