using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Data and Prefabs")]
    [SerializeField] private LevelsData levelsData;
    [SerializeField] private Ball ballPrefab;

    [Header("Scene References")]
    [SerializeField] private Transform ballsParent;
    [SerializeField] private TilesSpace tilesSpace;
    [SerializeField] private TextMeshProUGUI destroyedTilesScore;
    [SerializeField] private TextMeshProUGUI destructibleTilesScore;
    [SerializeField] private DualChoicePopUp endGamePopUp;
    [field: SerializeField] public PlayerBar PlayerBar { get; private set; }

    [Header("General Settings")]
    [SerializeField] private float ballSpawnOffset;
    [SerializeField] private Color defeatMessageColor;
    [SerializeField] private Color victoryMessageColor;
    [SerializeField] private string musicID;

    [Header("Buffs Settings")]
    [SerializeField] private float increaseSizeDuration;
    [field: SerializeField] public float IncreaseSizeMultiplier { get; private set; }
    [SerializeField] private uint extraBallsSpawned;
    [SerializeField] private float extraBallsSpawnAngle;
    [SerializeField] private float shotWhenHitDuration;
    [SerializeField] private float stickyDuration;
    [SerializeField] private float ballInvincibilityDuration;

    private List<Ball> balls = new();
    private Tile[,] tiles = null;
    private int destructibleTilesCount = 0;
    private int destroyedTiles = 0;
    private InputAction openMenuAction;

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
        AudioManager.Instance.PlayMusic(musicID);
        openMenuAction = InputSystem.actions.FindAction("Exit");
        openMenuAction.performed += ExplicitOpenEndGamePopUp;
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        foreach (Ball ball in balls) ball.OnBallDestroyed -= OnBallDestroyed;
        for (int column = 0; column < tiles.GetLength(0); column++)
        {
            for (int line = 0; line < tiles.GetLength(1); line++)
            {
                if (tiles[column, line] == null) continue;
                tiles[column, line].OnTileDestroyed -= OnTileDestroyed;
            }
        }
        openMenuAction.performed -= ExplicitOpenEndGamePopUp;
    }

    private void InitializeLevel()
    {
        LevelData currentLevel = SceneManager.Instance.CurrentLevel;

        SpawnBall(spawnsCaptured: true);

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

        destroyedTiles = 0;
        destroyedTilesScore.text = destroyedTiles.ToString();
        destructibleTilesScore.text = "/" + destructibleTilesCount.ToString();
    }

    public void SpawnBall(uint amount = 1, bool spawnsCaptured = false)
    {
        float spawnAngle = 90f + ((amount == 1) ? 0 : -extraBallsSpawnAngle / 2f);

        for (int i = 0; i < amount; i++)
        {
            Vector3 direction = new Vector3(Mathf.Cos(spawnAngle * Mathf.Deg2Rad), Mathf.Sin(spawnAngle * Mathf.Deg2Rad), 0f);
            Vector3 spawnPosition = PlayerBar.transform.position + direction * ballSpawnOffset;
            Ball ball = Instantiate(ballPrefab, spawnPosition, Quaternion.identity, ballsParent);
            if (spawnsCaptured) PlayerBar.CaptureBall(ball);
            else LaunchBall(ball, direction);
            balls.Add(ball);
            ball.OnBallDestroyed += OnBallDestroyed;
            spawnAngle += extraBallsSpawnAngle / (amount - 1);
        }
    }

    public void LaunchBall(Ball ball, Vector3 direction)
    {
        if (ball.transform.parent != ballsParent) ball.transform.SetParent(ballsParent);
        ball.Launch(direction);
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
                SpawnBall(extraBallsSpawned);
                break;

            case BuffType.Sticky:
                PlayerBar.AddStickyDuration(stickyDuration);
                break;

            case BuffType.ShootWhenHit:
                PlayerBar.AddShotWhenHitDuration(shotWhenHitDuration);
                break;

            case BuffType.InvincibleBall:
                foreach (Ball ball in balls) ball.AddInvincibilityDuration(ballInvincibilityDuration);
                break;
        }

        Destroy(buffCollectable.gameObject);
    }

    public void ExplicitOpenEndGamePopUp(InputAction.CallbackContext context) => ExplicitOpenEndGamePopUp();
    public void ExplicitOpenEndGamePopUp()
    {
        DualChoicePopUp.Settings settings = new();
        settings.message = "Exit Level?";
        settings.leftButtonLabel = "No";
        settings.rightButtonLabel = "Yes";
        endGamePopUp.Open(settings, OnGoBackPopUpCancel, GoBackToMainMenu);
    }
    private void OnGoBackPopUpCancel() => endGamePopUp.Close();
    private void GoBackToMainMenu() => SceneManager.Instance.GoToMainMenu();
    private void RetryLevel() => SceneManager.Instance.ReloadLevel();

    private void OnBallDestroyed(Ball ball)
    {
        ball.OnBallDestroyed -= OnBallDestroyed;
        balls.Remove(ball);

        // Defeat
        if (balls.Count == 0 && gameObject.scene.isLoaded)
        {
            DualChoicePopUp.Settings settings = new();
            settings.message = "Defeat";
            settings.messageColor = defeatMessageColor;
            settings.leftButtonLabel = "Retry";
            settings.rightButtonLabel = "Go to Main Menu";
            endGamePopUp.Open(settings, RetryLevel, GoBackToMainMenu);
        }
    }

    private void OnTileDestroyed(Tile tile, Tile.DamageEffect destroyEffect)
    {
        Vector2Int position = tile.GridPosition;

        tile.OnTileDestroyed -= OnTileDestroyed;
        tiles[position.x, position.y] = null;
        destroyedTiles++;
        destroyedTilesScore.text = destroyedTiles.ToString();

        // Victory
        if (destroyedTiles >= destructibleTilesCount)
        {
            PlayerPrefs.SetString(SceneManager.Instance.CurrentLevel.ID, "");
            PlayerPrefs.Save();
            DualChoicePopUp.Settings settings = new();
            settings.message = "Victory!";
            settings.messageColor = victoryMessageColor;
            settings.leftButtonLabel = "Play Again";
            settings.rightButtonLabel = "Go to  Main Menu";
            endGamePopUp.Open(settings, RetryLevel, GoBackToMainMenu);
            return;
        }
        ApplyDestructionEffects(position, destroyEffect);
    }

    private void ApplyDestructionEffects(Vector2Int position, Tile.DamageEffect destroyEffect)
    {
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