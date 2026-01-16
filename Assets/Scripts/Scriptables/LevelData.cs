using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
[Serializable]
public class LevelData : ScriptableObject, ISerializationCallbackReceiver
{
    public bool enabled;
    public string ID;

    public TilesData.TileData[,] tiles = null;
    public List<TilesData.TileData> tilesSerialized = null;

    public int[,] teste = null;
    public List<int> testeS = null;

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
        if (tiles == null)
        {
            tiles = new TilesData.TileData[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];
        }

        tilesSerialized = new();

        for (int i = 0; i < tiles.GetLength(0); i++)
        {
            for (int j = 0; j < tiles.GetLength(1); j++)
            {
                tilesSerialized.Add(tiles[i, j]);
            }
        }
    }

    public void PrintData()
    {
        StringBuilder print = new();
        for (int i = 0; i < LevelsData.LEVEL_GRID_SIZE_COLUMNS; i++)
        {
            for (int j = 0; j < LevelsData.LEVEL_GRID_SIZE_LINES; j++)
            {
                print.Append(teste[i, j]);
            }
            print.AppendLine();
        }
        Debug.LogError(print.ToString());
    }
}

[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelData myScript = (LevelData)target;

        if (GUILayout.Button("Print Data")) myScript.PrintData();
    }
}