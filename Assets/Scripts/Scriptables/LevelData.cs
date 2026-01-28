using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject, ISerializationCallbackReceiver
{
    public const string LEVEL_DATA_ASSET_PREFIX = "LevelData_";
    public const string LEVEL_DATA_ID_PREFIX = "Level_";

    public bool enabled;
    public string ID;

    public TilesData.TileData[,] tiles = null;
    [SerializeField] private List<TilesData.TileData> tilesSerialized = null;
    [SerializeField] private List<Vector2Int> tilesSerializedPosition = null;

    public void OnAfterDeserialize()
    {
        tiles = new TilesData.TileData[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];

        if (tilesSerialized == null) return;

        for (int i = 0; i < tilesSerialized.Count; i++)
        {
            tiles[tilesSerializedPosition[i].x, tilesSerializedPosition[i].y] = tilesSerialized[i];
        }
    }

    public void OnBeforeSerialize()
    {
        if (tiles == null) tiles = new TilesData.TileData[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];

        tilesSerialized = new();
        tilesSerializedPosition = new();

        for (int column = 0; column < tiles.GetLength(0); column++)
        {
            for (int line = 0; line < tiles.GetLength(1); line++)
            {
                if (tiles[column, line] == null) continue;
                tilesSerialized.Add(tiles[column, line]);
                tilesSerializedPosition.Add(new Vector2Int(column, line));
            }
        }
    }
}

[CustomEditor(typeof(LevelData)), CanEditMultipleObjects]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelData myScript = (LevelData)target;
    }
}