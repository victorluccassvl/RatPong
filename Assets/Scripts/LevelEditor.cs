using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEditor;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    private const string DROPDOWN_NULL_OPTION = "<None>";

    [Header("Data")]
    [SerializeField] private LevelsData levelsData;
    [SerializeField] private TilesData tilesData;
    [SerializeField] private string levelsFolderPath;

    [Header("References")]
    [Header("Level")]
    [SerializeField] private TMP_Dropdown levelDropdown;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button createButton;
    [SerializeField] private TMP_InputField createField;
    [SerializeField] private TextMeshProUGUI operationFeedback;

    [Header("Editor")]
    [SerializeField] private GridLayoutGroup tilesGrid;
    [SerializeField] private EditableTile tileTemplate;
    [SerializeField] private TextMeshProUGUI noLoadedLevelLabel;

    private EditableTile[,] editableTiles = null;


    private LevelData currentLevelData = null;
    private TilesData.TileData currentTile = null;

    private void Awake()
    {
        UpdateLevelOptions();
    }

    private void OnEnable()
    {
        levelDropdown.onValueChanged.AddListener(OnLevelSelect);
        saveButton.onClick.AddListener(OnSave);
        deleteButton.onClick.AddListener(OnDelete);
        createButton.onClick.AddListener(OnCreate);

        UpdateLevelTiles();
    }

    private void OnDisable()
    {
        levelDropdown.onValueChanged.RemoveListener(OnLevelSelect);
        saveButton.onClick.RemoveListener(OnSave);
        deleteButton.onClick.RemoveListener(OnDelete);
        createButton.onClick.RemoveListener(OnCreate);
    }

    private void OnSave()
    {

    }

    private void OnDelete()
    {

    }

    private void OnCreate()
    {

    }

    private void OnLevelSelect(int newOption)
    {
        string newOptionID = levelDropdown.options[newOption].text;
        currentLevelData = levelsData.levels.Find(levelData => levelData.ID == newOptionID);
        UpdateLevelTiles();
    }

    private void OnTileInteract(EditableTile tile, int column, int line)
    {

    }

    private void UpdateLevelOptions()
    {
        UpdateLevelsData();

        levelDropdown.ClearOptions();
        List<string> levelOptions = new() { DROPDOWN_NULL_OPTION };
        foreach (LevelData levelData in levelsData.levels)
        {
            levelOptions.Add(levelData.ID);
        }

        levelDropdown.AddOptions(levelOptions);
    }

    private void UpdateLevelsData()
    {
        if (levelsFolderPath == null || !Directory.Exists(levelsFolderPath))
        {
            Debug.LogWarning($"Invalid levels folder path {levelsFolderPath}. LevelsData not updated");
        }
        else
        {
            List<string> levelPaths = Directory.GetFiles(levelsFolderPath, "*.asset").ToList();

            levelsData.levels.Clear();

            foreach (string levelPath in levelPaths)
            {
                Object levelDataObject = AssetDatabase.LoadAssetAtPath<Object>(levelPath);

                if (levelDataObject is LevelData levelData)
                {
                    levelsData.levels.Add(levelData);
                }
                else
                {
                    AssetDatabase.DeleteAsset(levelPath);
                }
            }
        }
    }

    private void UpdateLevelTiles()
    {
        if (currentLevelData == null)
        {
            noLoadedLevelLabel.gameObject.SetActive(true);
            tilesGrid.gameObject.SetActive(false);
        }
        else
        {
            noLoadedLevelLabel.gameObject.SetActive(false);
            tilesGrid.gameObject.SetActive(true);

            if (editableTiles != null)
            {
                for (int i = 0; i < editableTiles.GetLength(0); i++)
                {
                    for (int j = 0; j < editableTiles.GetLength(1); j++)
                    {
                        editableTiles[i, j].OnTileSelected -= OnTileInteract;
                        Debug.LogError("ué");
                        Destroy(editableTiles[i, j].gameObject);
                    }
                }
            }

            editableTiles = new EditableTile[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];

            Vector2 tileSize;
            Vector2Int gridSize = new Vector2Int(LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES);
            tileSize = ((RectTransform)tilesGrid.transform).sizeDelta;
            tileSize -= new Vector2(tilesGrid.padding.left + tilesGrid.padding.right, tilesGrid.padding.top + tilesGrid.padding.bottom);
            tileSize -= (gridSize + Vector2.one) * tilesGrid.spacing;
            tileSize /= gridSize;

            tilesGrid.cellSize = tileSize;
            tilesGrid.constraintCount = (tilesGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount) ? LevelsData.LEVEL_GRID_SIZE_COLUMNS : LevelsData.LEVEL_GRID_SIZE_LINES;

            for (int column = 0; column < LevelsData.LEVEL_GRID_SIZE_COLUMNS; column++)
            {
                for (int line = 0; line < LevelsData.LEVEL_GRID_SIZE_LINES; line++)
                {
                    EditableTile newTile = Instantiate(tileTemplate, tilesGrid.transform);
                    newTile.Setup(tileSize, line, column, currentLevelData.tiles[column, line]);
                    newTile.OnTileSelected += OnTileInteract;
                    editableTiles[column, line] = newTile;
                }
            }
        }
    }
}
