using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Structure", menuName = "Game of Life/Structure")]
public class StructureData : ScriptableObject
{
    public string structureName;
    public int width = 3;
    public int height = 3;
    
    [TextArea(3, 5)]
    public string description;
    
    [Header("Cell Pattern (use X for alive, . for dead)")]
    [TextArea(3, 10)]
    public string pattern = "X.X\n.X.\nXXX";
    
    [System.NonSerialized]
    public bool[,] cells;

    public void InitializeCells()
    {
        if (!string.IsNullOrEmpty(pattern))
        {
            ParsePattern();
        }
        else
        {
            // Create empty cells as fallback
            cells = new bool[width, height];
        }
    }

    private void ParsePattern()
    {
        string[] rows = pattern.Split('\n');
        height = rows.Length;
        width = 0;
        
        // Find maximum width
        foreach (string row in rows)
        {
            if (row.Trim().Length > width)
                width = row.Trim().Length;
        }
        
        cells = new bool[width, height];
        
        for (int y = 0; y < height; y++)
        {
            string row = rows[height - 1 - y].Trim(); // Reverse Y so pattern appears correctly
            for (int x = 0; x < row.Length; x++)
            {
                char c = row[x];
                cells[x, y] = (c == 'X' || c == 'x' || c == '1');
            }
        }
    }

    [ContextMenu("Print Pattern Info")]
    public void PrintPatternInfo()
    {
        InitializeCells();
        Debug.Log($"Structure: {structureName}\nSize: {width}x{height}\nPattern:\n{PatternToString()}");
    }

    public string PatternToString()
    {
        if (cells == null) InitializeCells();
        
        string result = "";
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                result += cells[x, y] ? "X" : ".";
            }
            result += "\n";
        }
        return result;
    }
}