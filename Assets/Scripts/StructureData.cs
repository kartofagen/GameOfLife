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
                Cells[x, y] = c is 'X' or 'x' or '1';
            }
        }
    }
}