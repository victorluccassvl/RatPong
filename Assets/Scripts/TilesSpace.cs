using UnityEngine;

public class TilesSpace : MonoBehaviour
{
    [SerializeField] private Transform limit;

    public Vector2 AreaSize { get; private set; }
    public Vector2 CellSize { get; private set; }

    private void Awake()
    {
        Vector2Int gridSize = new Vector2Int(LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES);
        AreaSize = limit.position - transform.position;
        CellSize = AreaSize / gridSize;
    }

    public Vector3 GetLocalPositionForTile(Vector2Int gridPosition)
    {
        return new Vector3(CellSize.x * (0.5f + gridPosition.x), CellSize.y * (0.5f + gridPosition.y));
    }

    private void OnDrawGizmos()
    {
        Vector2Int gridSize = new Vector2Int(LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES);
        Vector2 areaSize = limit.position - transform.position;
        Vector2 cellSize = areaSize / gridSize;

        Vector3 v1 = transform.position;
        Vector3 v2 = v1 + Vector3.up * areaSize.y;
        Vector3 v3 = v2 + Vector3.right * areaSize.x;
        Vector3 v4 = v3 + Vector3.down * areaSize.y;

        Gizmos.color = Color.darkRed;
        Gizmos.DrawLineStrip(new Vector3[4] { v1, v2, v3, v4 }, true);

        for (int line = 1; line < gridSize.y; line++)
        {
            v1 = transform.position + Vector3.up * cellSize.y * line;
            v2 = v1 + Vector3.right * areaSize.x;
            Gizmos.DrawLine(v1, v2);
        }

        for (int column = 1; column < gridSize.x; column++)
        {
            v1 = transform.position + Vector3.right * cellSize.x * column;
            v2 = v1 + Vector3.up * areaSize.y;
            Gizmos.DrawLine(v1, v2);
        }
    }
}
