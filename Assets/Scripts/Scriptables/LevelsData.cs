using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "LevelsData", menuName = "Scriptable Objects/LevelsData")]
public class LevelsData : ScriptableObject
{
    public const int LEVEL_GRID_SIZE_COLUMNS = 12;
    public const int LEVEL_GRID_SIZE_LINES = 12;

    public List<LevelData> levels;
}

[CustomEditor(typeof(LevelsData))]
public class LevelsDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        LevelsData myScript = (LevelsData)target;
    }
}