using System;
using UnityEngine;
using UnityEngine.UI;
using KBCore.Refs;

public class EditableTile : MonoBehaviour
{
    [SerializeField, Self] private Button button;

    private TilesData.TileData currentTile;
    private int column;
    private int line;

    public Action<EditableTile, int, int> OnTileSelected = delegate { };

    public void Onable()
    {
        button.onClick.AddListener(OnClick);
    }

    public void OnDisable()
    {
        button.onClick.RemoveListener(OnClick);
    }

    private void OnClick() => OnTileSelected(this, column, line);

    public void Setup(Vector2 size, int column, int line, TilesData.TileData tileData)
    {
        currentTile = tileData;

        this.line = line;
        this.column = column;

        gameObject.name = $"Tile[{column}][{line}]";

        if (currentTile == null || currentTile.levelEditorRepresentation == null)
        {
            //button.image.sprite = null;
            // button.image.color = Color.clear;
        }
        else
        {
            button.image.sprite = currentTile.levelEditorRepresentation;
            button.image.color = Color.white;
        }
    }
}
