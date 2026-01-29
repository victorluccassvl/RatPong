using UnityEngine;
using UnityEngine.UI;
using KBCore.Refs;

[ExecuteInEditMode]
public class GridLayoutFitter : MonoBehaviour
{
    [SerializeField, Self] private GridLayoutGroup gridLayoutGroup;
    [SerializeField, Self] private RectTransform rectTransform;

    private Vector3[] corners = new Vector3[4];
    private Vector2 lastResolution = Vector2.zero;

    public void Update()
    {
        if (lastResolution.x == Screen.width && lastResolution.y == Screen.height) return;

        lastResolution = new Vector2(Screen.width, Screen.height);

        rectTransform.GetLocalCorners(corners);

        if (gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedRowCount)
        {
            float height = corners[1].y - corners[2].y;
            float cellHeight = height - gridLayoutGroup.padding.bottom - gridLayoutGroup.padding.top;
            cellHeight -= gridLayoutGroup.spacing.y * (gridLayoutGroup.constraintCount - 1);
            cellHeight /= gridLayoutGroup.constraintCount;
            gridLayoutGroup.cellSize = new Vector2(gridLayoutGroup.cellSize.x, cellHeight);
        }
        else
        {
            float width = corners[2].x - corners[1].x;
            float cellWidth = width - gridLayoutGroup.padding.left - gridLayoutGroup.padding.right;
            cellWidth -= gridLayoutGroup.spacing.x * (gridLayoutGroup.constraintCount - 1);
            cellWidth /= gridLayoutGroup.constraintCount;
            gridLayoutGroup.cellSize = new Vector2(cellWidth, gridLayoutGroup.cellSize.y);
        }
    }
}
