using UnityEngine;

[CreateAssetMenu(fileName = "New Zone Structure", menuName = "Game of Life/Zone Structure")]
public class ZoneStructureData : ScriptableObject
{
    public string structureName;
    public int width = 3;
    public int height = 3;
    
    [TextArea(3, 5)]
    public string description;
    
    [Header("Zone Pattern (use X for zone, . for empty)")]
    [TextArea(3, 10)]
    public string pattern = "XXX\nXXX\nXXX";
    
    [Header("Zone Rules")]
    [Range(0, 8)] public int minSurviveNeighbors = 2;
    [Range(0, 8)] public int maxSurviveNeighbors = 3;
    [Range(0, 8)] public int reproduceNeighbors = 3;
    public Color zoneColor = new Color(1f, 0f, 0f, 0.3f);
    
    [System.NonSerialized]
    public bool[,] Cells;

    public void InitializeCells()
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            ParsePattern();
        }
        else
        {
            Cells = new bool[width, height];
        }
    }

    private void ParsePattern()
    {
        string[] rows = pattern.Split('\n');
        height = rows.Length;
        width = 0;
        
        foreach (string row in rows)
        {
            if (row.Trim().Length > width)
                width = row.Trim().Length;
        }
        
        Cells = new bool[width, height];
        
        for (int y = 0; y < height; ++y)
        {
            string row = rows[height - 1 - y].Trim();
            for (int x = 0; x < row.Length; ++x)
            {
                char c = row[x];
                Cells[x, y] = (c == 'X' || c == 'x' || c == '1');
            }
        }
    }

    [ContextMenu("Print Pattern Info")]
    public void PrintPatternInfo()
    {
        InitializeCells();
        Debug.Log($"Zone Structure: {structureName}\nSize: {width}x{height}\nPattern:\n{PatternToString()}");
    }

    private string PatternToString()
    {
        if (Cells == null) InitializeCells();
        
        string result = "";
        for (int y = height - 1; y >= 0; --y)
        {
            for (int x = 0; x < width; ++x)
            {
                result += Cells[x, y] ? "X" : ".";
            }
            result += "\n";
        }
        return result;
    }
}