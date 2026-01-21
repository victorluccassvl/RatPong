using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEditor;
using System.Text;

[CreateAssetMenu(fileName = "LevelsData", menuName = "Scriptable Objects/LevelsData")]
public class LevelsData : ScriptableObject
{
    public const int LEVEL_GRID_SIZE_COLUMNS = 12;
    public const int LEVEL_GRID_SIZE_LINES = 8;

    public List<LevelData> levels;

    public void PrintData()
    {
        StringBuilder print = new();
        print.AppendLine("Markers");
        print.AppendLine("[C] Common");
        print.AppendLine();

        foreach (LevelData levelData in levels)
        {
            print.AppendLine($"Level:{levelData.name}");
            for (int i = 0; i < LEVEL_GRID_SIZE_COLUMNS; i++)
            {
                for (int j = 0; j < LEVEL_GRID_SIZE_LINES; j++)
                {
                    if (levelData.tiles[i, j] == null)
                    {
                        print.Append("[ ]");
                        continue;
                    }
                    switch (levelData.tiles[i, j].variant)
                    {
                        case Tile.Variant.Common:
                            print.AppendFormat("[C]");
                            break;
                        default:
                            print.AppendFormat("[?]");
                            break;
                    }

                }
                print.AppendLine();
            }
        }

        Debug.LogError(print.ToString());
    }
}

[CustomEditor(typeof(LevelsData))]
public class LevelsDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelsData myScript = (LevelsData)target;

        if (GUILayout.Button("Print Data")) myScript.PrintData();
    }
}