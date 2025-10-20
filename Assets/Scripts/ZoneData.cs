using UnityEngine;

[CreateAssetMenu(fileName = "New Zone", menuName = "Game of Life/Zone Data")]
public class ZoneData : ScriptableObject
{
    public string zoneName;
    public ZoneShape shape = ZoneShape.Square;
    public int size = 3;
    public Color zoneColor = new Color(1f, 0f, 0f, 0.3f);
    
    [Header("Rules")]
    [Range(0, 8)] public int minSurviveNeighbors = 2;
    [Range(0, 8)] public int maxSurviveNeighbors = 3;
    [Range(0, 8)] public int reproduceNeighbors = 3;
    
    public enum ZoneShape
    {
        Square,
        Circle,
        Diamond
    }
    
    public bool[,] GetZoneCells(int gridWidth, int gridHeight)
    {
        bool[,] cells = new bool[gridWidth, gridHeight];
        
        switch (shape)
        {
            case ZoneShape.Square:
                CreateSquareZone(cells, gridWidth, gridHeight);
                break;
            case ZoneShape.Circle:
                CreateCircleZone(cells, gridWidth, gridHeight);
                break;
            case ZoneShape.Diamond:
                CreateDiamondZone(cells, gridWidth, gridHeight);
                break;
        }
        
        return cells;
    }
    
    private void CreateSquareZone(bool[,] cells, int gridWidth, int gridHeight)
    {
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        int halfSize = size / 2;
        
        for (int x = centerX - halfSize; x <= centerX + halfSize; x++)
        {
            for (int y = centerY - halfSize; y <= centerY + halfSize; y++)
            {
                if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
                {
                    cells[x, y] = true;
                }
            }
        }
    }
    
    private void CreateCircleZone(bool[,] cells, int gridWidth, int gridHeight)
    {
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        float radius = size / 2f;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                if (distance <= radius)
                {
                    cells[x, y] = true;
                }
            }
        }
    }
    
    private void CreateDiamondZone(bool[,] cells, int gridWidth, int gridHeight)
    {
        int centerX = gridWidth / 2;
        int centerY = gridHeight / 2;
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                int dx = Mathf.Abs(x - centerX);
                int dy = Mathf.Abs(y - centerY);
                if (dx + dy <= size / 2)
                {
                    cells[x, y] = true;
                }
            }
        }
    }
}