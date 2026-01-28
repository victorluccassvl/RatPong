using System;
using UnityEngine;
using UnityEngine.UI;
using KBCore.Refs;

public class EditableTile : MonoBehaviour
{
    [SerializeField, Self] private Button button;

    public TilesData.TileData CurrentTile { get; private set; }
    private int column;
    private int line;

    public Action<EditableTile, int, int> OnTileSelected = delegate { };

    public void OnClick() => OnTileSelected(this, column, line);

    public void Setup(int column, int line, TilesData.TileData tileData)
    {
        CurrentTile = tileData;

        this.line = line;
        this.column = column;

        gameObject.name = $"Tile[{column}][{line}]";

        Setup(tileData);
    }

    public void Setup(TilesData.TileData tileData)
    {
        CurrentTile = tileData;

        button.image.sprite = CurrentTile?.levelEditorRepresentation;
    }
}
