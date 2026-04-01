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

    [Header("General Settings")]
    [SerializeField] private float ballSpawnOffset;

    [Header("Buffs Settings")]
    [SerializeField] private float increaseSizeDuration;
    [field: SerializeField] public float IncreaseSizeMultiplier { get; private set; }
    [SerializeField] private uint extraBallsSpawned;
    [SerializeField] private float extraBallsSpawnedLifespam;
    [SerializeField] private float shotWhenHitDuration;
    [SerializeField] private float stickyDuration;
    [SerializeField] private float ballInvincibilityDuration;

    private List<Ball> balls = new();
    private Tile[,] tiles = null;
    private int destructibleTilesCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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

        SpawnBall();

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

                Tile newTile = Instantiate(tile.prefab, tilesSpace.transform).GetComponent<Tile>();
                if (newTile.IsDestructible) destructibleTilesCount++;

                Vector2Int gridPosition = new Vector2Int(column, line);
                newTile.Setup(gridPosition, tilesSpace);
                newTile.OnTileDestroyed += OnTileDestroyed;

                tiles[column, line] = newTile;
            }
        }
    }

    public void SpawnBall(float? lifespam = null)
    {
        Ball ball = Instantiate(ballPrefab, PlayerBar.transform.position + Vector3.up * ballSpawnOffset, Quaternion.identity, ballsParent);
        balls.Add(ball);
        if (lifespam != null) ball.SetLifespam(lifespam.Value);
        ball.OnBallDestroyed += OnBallDestroyed;
    }

    public void CollectBuff(BuffCollectable buffCollectable)
    {
        BuffType buff = buffCollectable.Buff;
        switch (buff)
        {
            case BuffType.IncreaseSize:
                PlayerBar.AddIncreasedSizeDuration(increaseSizeDuration);
                break;

            case BuffType.SpawnBalls:
                for (int i = 0; i < extraBallsSpawned; i++) SpawnBall(extraBallsSpawnedLifespam);
                break;

            case BuffType.Sticky:
                PlayerBar.AddStickyDuration(stickyDuration);
                break;

            case BuffType.ShootWhenHit:
                PlayerBar.AddShotWhenHitDuration(shotWhenHitDuration);
                break;

            case BuffType.InvincibleBall:
                foreach (Ball ball in balls) ball.SetInvincibilityDuration(ballInvincibilityDuration);
                break;
        }

        Destroy(buffCollectable.gameObject);
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

    private void OnTileDestroyed(Tile tile, Tile.DamageEffect destroyEffect)
    {
        Vector2Int position = tile.GridPosition;

        tile.OnTileDestroyed -= OnTileDestroyed;
        tiles[position.x, position.y] = null;
        destructibleTilesCount--;

        if (destructibleTilesCount <= 0)
        {
            // Vitoria
            PlayerPrefs.SetString(SceneManager.Instance.CurrentLevel.ID, "");
            PlayerPrefs.Save();
            SceneManager.Instance.GoToMainMenu();
        }

        bool stop = false;
        bool destroyLine = (destroyEffect & Tile.DamageEffect.DamageLine) != 0;
        bool destroyColumn = (destroyEffect & Tile.DamageEffect.DamageColumn) != 0;
        bool destroyDiagonals = (destroyEffect & Tile.DamageEffect.DamageDiagonals) != 0;
        for (int i = 1; !stop; i++)
        {
            bool hasUpperLine = position.y + i < LevelsData.LEVEL_GRID_SIZE_LINES;
            bool hasLowerLine = position.y - i >= 0;
            bool hasRightColumn = position.x + i < LevelsData.LEVEL_GRID_SIZE_COLUMNS;
            bool hasLeftColumn = position.x - i >= 0;
            stop = true;

            if (destroyLine)
            {
                if (hasRightColumn) { tiles[position.x + i, position.y]?.Hit(position); stop = false; }
                if (hasLeftColumn) { tiles[position.x - i, position.y]?.Hit(position); stop = false; }
            }

            if (destroyColumn)
            {
                if (hasUpperLine) { tiles[position.x, position.y + i]?.Hit(position); stop = false; }
                if (hasLowerLine) { tiles[position.x, position.y - i]?.Hit(position); stop = false; }
            }

            if (destroyDiagonals)
            {
                if (hasRightColumn)
                {
                    if (hasUpperLine) { tiles[position.x + i, position.y + i]?.Hit(position); stop = false; }
                    if (hasLowerLine) { tiles[position.x + i, position.y - i]?.Hit(position); stop = false; }
                }
                if (hasLeftColumn)
                {
                    if (hasUpperLine) { tiles[position.x - i, position.y + i]?.Hit(position); stop = false; }
                    if (hasLowerLine) { tiles[position.x - i, position.y - i]?.Hit(position); stop = false; }
                }
            }
        }
    }
}