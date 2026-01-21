using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using UnityEditor;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    private const string DROPDOWN_NULL_OPTION = "<None>";

    private enum OperationFeedback
    {
        LevelCreated,
        LevelDeleted,
        LevelSaved,
        LevelLoaded,
        FailedOperation
    }

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

    [Header("Picker")]
    [SerializeField] private GridLayoutGroup pickerGrid;
    [SerializeField] private TilePickerOption pickerOptionTemplate;
    [SerializeField] private TextMeshProUGUI currentPickedLabel;

    private EditableTile[,] editableTiles = null;
    private List<TilePickerOption> pickerOptions = new();

    private LevelData currentLevelData = null;
    private TilePickerOption currentTilePickedOption = null;

    private void Awake()
    {
        operationFeedback.text = "";

        UpdateLevelsData();
        UpdateLevelOptions();
        UpdateTilePickerOptions();
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

    private bool AssertLevelsDataPathExists()
    {
        if (levelsFolderPath == null || !AssetDatabase.IsValidFolder(levelsFolderPath))
        {
            Debug.LogWarning($"Invalid levels folder path {levelsFolderPath}.");
            return false;
        }
        return true;
    }

    private void OnSave()
    {
        if (currentLevelData == null) return;
        if (!AssertLevelsDataPathExists()) return;

        for (int column = 0; column < LevelsData.LEVEL_GRID_SIZE_COLUMNS; column++)
        {
            for (int line = 0; line < LevelsData.LEVEL_GRID_SIZE_LINES; line++)
            {
                currentLevelData.tiles[column, line] = editableTiles[column, line].CurrentTile;
            }
        }
        AssetDatabase.SaveAssetIfDirty(currentLevelData);

        DisplayOperationFeedback(OperationFeedback.LevelSaved, currentLevelData.ID);
    }

    private void OnDelete()
    {
        if (currentLevelData == null) return;
        if (!AssertLevelsDataPathExists()) return;

        string levelToDeleteID = currentLevelData.ID;
        AssetDatabase.DeleteAsset($"{levelsFolderPath}\\{currentLevelData.name}.asset");
        UpdateLevelsData();
        UpdateLevelOptions();
        OnLevelSelect(0);

        DisplayOperationFeedback(OperationFeedback.LevelDeleted, levelToDeleteID);
    }

    private void OnCreate()
    {
        if (!AssertLevelsDataPathExists()) return;

        LevelData newLevel = ScriptableObject.CreateInstance<LevelData>();
        newLevel.ID = $"{LevelData.LEVEL_DATA_ID_PREFIX}{createField.text}";
        AssetDatabase.CreateAsset(newLevel, $"{levelsFolderPath}\\{LevelData.LEVEL_DATA_ASSET_PREFIX}{createField.text}.asset");

        UpdateLevelsData();
        UpdateLevelOptions();

        DisplayOperationFeedback(OperationFeedback.LevelCreated, newLevel.ID);
    }

    private void OnLevelSelect(int newOption)
    {
        string newOptionID = levelDropdown.options[newOption].text;
        currentLevelData = levelsData.levels.Find(levelData => levelData.ID == newOptionID);
        UpdateLevelTiles();
        if (newOptionID != DROPDOWN_NULL_OPTION) DisplayOperationFeedback(OperationFeedback.LevelLoaded, newOptionID);
    }

    private void OnTileInteract(EditableTile tile, int column, int line)
    {
        if (currentTilePickedOption != null && tile.CurrentTile != null && currentTilePickedOption.Option == tile.CurrentTile)
        {
            tile.Setup(null);
            return;
        }

        tile.Setup(currentTilePickedOption?.Option);
    }

    private void OnOptionPicked(TilePickerOption optionPicked)
    {
        if (optionPicked.Option == null)
        {
            currentTilePickedOption = null;
            currentPickedLabel.text = "";
            return;
        }

        currentTilePickedOption = optionPicked;
        currentPickedLabel.text = optionPicked.Option.variant.ToString();
    }

    private void UpdateLevelOptions()
    {
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
        if (!AssertLevelsDataPathExists()) return;

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
                        Destroy(editableTiles[i, j].gameObject);
                    }
                }
            }

            editableTiles = new EditableTile[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];

            Vector2 tileSize;
            Vector2Int gridSize = new Vector2Int(LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES);
            tileSize = ((RectTransform)tilesGrid.transform).sizeDelta / new Vector2(1f, 2f);
            tileSize -= new Vector2(tilesGrid.padding.left + tilesGrid.padding.right, tilesGrid.padding.top + tilesGrid.padding.bottom);
            tileSize -= (gridSize + Vector2.one) * tilesGrid.spacing;
            tileSize /= gridSize;

            tilesGrid.cellSize = tileSize;
            tilesGrid.constraintCount = (tilesGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount) ? LevelsData.LEVEL_GRID_SIZE_COLUMNS : LevelsData.LEVEL_GRID_SIZE_LINES;
            pickerGrid.cellSize = tileSize;

            for (int column = 0; column < LevelsData.LEVEL_GRID_SIZE_COLUMNS; column++)
            {
                for (int line = 0; line < LevelsData.LEVEL_GRID_SIZE_LINES; line++)
                {
                    EditableTile newTile = Instantiate(tileTemplate, tilesGrid.transform);
                    newTile.Setup(line, column, currentLevelData.tiles[column, line]);
                    newTile.OnTileSelected += OnTileInteract;
                    editableTiles[column, line] = newTile;
                }
            }
        }
    }

    private void UpdateTilePickerOptions()
    {
        foreach (TilePickerOption option in pickerOptions) Destroy(option.gameObject);
        pickerOptions = new();

        foreach (TilesData.TileData tileData in tilesData.tilesData)
        {
            TilePickerOption newPickerOption = Instantiate(pickerOptionTemplate, pickerGrid.transform);
            newPickerOption.Setup(tileData);
            newPickerOption.OnOptionPicked += OnOptionPicked;
            pickerOptions.Add(newPickerOption);
        }

        TilePickerOption clearPickerOption = Instantiate(pickerOptionTemplate, pickerGrid.transform);
        clearPickerOption.Setup(null);
        clearPickerOption.OnOptionPicked += OnOptionPicked;
        pickerOptions.Add(clearPickerOption);

        OnOptionPicked(clearPickerOption);
    }

    private Coroutine displayOperationCoroutine = null;
    private void DisplayOperationFeedback(OperationFeedback operation, params string[] args)
    {
        if (displayOperationCoroutine != null)
        {
            StopCoroutine(displayOperationCoroutine);
            displayOperationCoroutine = null;
        }

        displayOperationCoroutine = StartCoroutine(DisplayOperationFeedbackRoutine(operation, args));
    }
    private IEnumerator DisplayOperationFeedbackRoutine(OperationFeedback operation, params string[] args)
    {
        switch (operation)
        {
            case OperationFeedback.LevelCreated:
                operationFeedback.color = Color.green;
                operationFeedback.text = $"Level [{args[0]}] Created";
                break;
            case OperationFeedback.LevelDeleted:
                operationFeedback.color = Color.red;
                operationFeedback.text = $"Level [{args[0]}] Deleted";
                break;
            case OperationFeedback.LevelSaved:
                operationFeedback.color = Color.green;
                operationFeedback.text = $"Level [{args[0]}] Saved";
                break;
            case OperationFeedback.LevelLoaded:
                operationFeedback.color = Color.green;
                operationFeedback.text = $"Level [{args[0]}] Loaded";
                break;
            case OperationFeedback.FailedOperation:
                operationFeedback.text = "";
                break;
        }

        yield return new WaitForSeconds(2f);
        operationFeedback.text = "";
    }
}
