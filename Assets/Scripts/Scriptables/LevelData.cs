using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
[Serializable]
public class LevelData : ScriptableObject, ISerializationCallbackReceiver
{
    public const string LEVEL_DATA_ASSET_PREFIX = "LevelData_";
    public const string LEVEL_DATA_ID_PREFIX = "Level_";

    public bool enabled;
    public string ID;

    public TilesData.TileData[,] tiles = null;
    [SerializeField, HideInInspector] private List<TilesData.TileData> tilesSerialized = null;

    public void OnAfterDeserialize()
    {
        tiles = new TilesData.TileData[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];
        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                tiles[i, j] = tilesSerialized[i * LevelsData.LEVEL_GRID_SIZE_LINES + j];
            }
        }
    }

    public void OnBeforeSerialize()
    {
        if (tiles == null) tiles = new TilesData.TileData[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];

        tilesSerialized = new();

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                tilesSerialized.Add(tiles[i, j]);
            }
        }
    }
}

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //LevelData myScript = (LevelData)target;
    }
}