using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data and Prefabs")]
    [SerializeField] private LevelsData levelsData;
    [SerializeField] private Ball ballPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform ballsParent;
    [SerializeField] private TilesSpace tilesSpace;
    [field: SerializeField] public PlayerBar PlayerBar { get; private set; }

    [Header("Settings")]
    [SerializeField] private float ballSpawnOffset;

    private List<Ball> balls = new();
    private Tile[,] tiles = null;
    private int tilesCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitializeLevel();
    }

    private void InitializeLevel()
    {
        LevelData currentLevel = SceneManager.Instance.CurrentLevel;

        Ball ball = Instantiate(ballPrefab, PlayerBar.transform.position + Vector3.up * ballSpawnOffset, Quaternion.identity, ballsParent);
        balls.Add(ball);
        ball.OnBallDestroyed += OnBallDestroyed;

        tiles = new Tile[LevelsData.LEVEL_GRID_SIZE_COLUMNS, LevelsData.LEVEL_GRID_SIZE_LINES];
        for (int column = 0; column < currentLevel.tiles.GetLength(0); column++)
        {
            for (int line = 0; line < currentLevel.tiles.GetLength(1); line++)
            {
                TilesData.TileData tile = currentLevel.tiles[column, line];
                if (tile == null)
                {
                    tiles[column, line] = null;
                    continue;
                }

                tilesCount++;
                Tile newTile = Instantiate(tile.prefab, tilesSpace.transform).GetComponent<Tile>();
                Vector2Int gridPosition = new Vector2Int(column, line);
                newTile.Setup(gridPosition, tilesSpace);

                tiles[column, line] = newTile;
            }
        }
    }

    private void OnBallDestroyed(Ball ball)
    {
        ball.OnBallDestroyed -= OnBallDestroyed;
        balls.Remove(ball);

        if (balls.Count == 0)
        {
            // Derrota
            SceneManager.Instance.GoToMainMenu();
        }
    }

    private void OnTileDestroyed(Tile tile)
    {
        tile.OnTileDestroyed -= OnTileDestroyed;
        tiles[tile.GridPosition.x, tile.GridPosition.y] = null;
        tilesCount--;

        if (tilesCount <= 0)
        {
            // Vitoria
            PlayerPrefs.SetString(SceneManager.Instance.CurrentLevel.ID, "");
            PlayerPrefs.Save();
            SceneManager.Instance.GoToMainMenu();
        }
    }
}