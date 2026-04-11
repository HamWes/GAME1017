using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    InMenu,
    InGame,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
    private const string StartSceneName = "StartScene";
    private const string GameSceneName = "GameScene";
    private const string GameOverSceneName = "GameOverScene";

    // StartScene owns the shared managers and they persist across the rest of the run.
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private float difficultyIncreaseInterval = 5f;
    [SerializeField] private float speedIncreasePercent = 0.1f;
    [SerializeField] private float gapIncreaseAmount = 0.1f;
    [SerializeField] private float maxGapSize = 2.5f;

    // This flag lets the title scene request a run before GameScene has finished loading.
    private bool startGameOnSceneLoad;
    private float nextDifficultyTime;
    private float originalPlayerSpeed;
    private float originalMinGap;
    private float originalMaxGap;
    private bool hasDifficultyDefaults;
    private Coroutine gameOverRoutine;
    private PlayerController player;
    private SegmentSpawner segmentSpawner;
    private BackgroundManager backgroundManager;

    public GameState CurrentGameState { get; private set; } = GameState.InMenu;
    public float CurrentRunTime { get; private set; }
    public float LastRunTime { get; private set; }
    public float BestTime { get; private set; }
    public UIManager UIManager => uiManager;
    public SoundManager SoundManager => soundManager;

    private bool TimerRunning => CurrentGameState == GameState.InGame;
    private readonly List<float> leaderboardScores = new();

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        LoadSavedScores();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this)
        {
            return;
        }

        HandleSceneLoaded(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        if (!TimerRunning)
        {
            return;
        }

        // The run timer only advances during active gameplay.
        CurrentRunTime += Time.deltaTime;
        IncreaseDifficulty();
        UIManager?.UpdateTimerDisplay(CurrentRunTime);
    }

    public void PlayGame()
    {
        startGameOnSceneLoad = true;

        if (SceneManager.GetActiveScene().name == GameSceneName)
        {
            StartRun(resetTimer: true);
            return;
        }

        SceneManager.LoadScene(GameSceneName);
    }

    public void RestartGame()
    {
        if (SceneManager.GetActiveScene().name != GameSceneName)
        {
            return;
        }

        CurrentRunTime = 0f;
        nextDifficultyTime = difficultyIncreaseInterval;
        startGameOnSceneLoad = false;
        ResetDifficulty();
        player?.ResetPlayer();
        backgroundManager?.Initialize();
        SetGameState(GameState.InMenu);
        UIManager?.InitializeForMenu();
    }

    public void GameOver()
    {
        if (CurrentGameState == GameState.GameOver || gameOverRoutine != null)
        {
            return;
        }

        LastRunTime = CurrentRunTime;
        SetGameState(GameState.GameOver);
        gameOverRoutine = StartCoroutine(HandleGameOverTransition());
    }

    public void ReturnToTitle()
    {
        startGameOnSceneLoad = false;
        CurrentRunTime = 0f;
        nextDifficultyTime = difficultyIncreaseInterval;
        SetGameState(GameState.InMenu);
        SceneManager.LoadScene(StartSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this)
        {
            return;
        }

        HandleSceneLoaded(scene);
    }

    private void HandleSceneLoaded(Scene scene)
    {
        UIManager?.BindSceneButtons(scene);

        if (scene.name == GameSceneName)
        {
            // Gameplay objects are rebound whenever GameScene loads.
            player = FindFirstObjectByType<PlayerController>();
            segmentSpawner = FindFirstObjectByType<SegmentSpawner>();
            backgroundManager = FindFirstObjectByType<BackgroundManager>();
        }
        else
        {
            player = null;
            segmentSpawner = null;
            backgroundManager = null;
        }

        switch (scene.name)
        {
            case StartSceneName:
                SetGameState(GameState.InMenu);
                UIManager?.InitializeForMenu();
                UIManager?.UpdateLeaderboardDisplay(BestTime, leaderboardScores);
                break;

            case GameSceneName:
                if (startGameOnSceneLoad)
                {
                    StartRun(resetTimer: true);
                }
                break;

            case GameOverSceneName:
                SetGameState(GameState.GameOver);
                UIManager?.ShowGameOver(LastRunTime);
                UIManager?.UpdateLeaderboardDisplay(BestTime, leaderboardScores);
                break;
        }
    }

    private void StartRun(bool resetTimer)
    {
        if (resetTimer)
        {
            CurrentRunTime = 0f;
        }

        ResetDifficulty();
        nextDifficultyTime = difficultyIncreaseInterval;

        SetGameState(GameState.InGame);

        player?.ResetPlayer();
        player?.Initialize();
        segmentSpawner?.Initialize();
        backgroundManager?.Initialize();
        UIManager?.ShowRunStarted(CurrentRunTime);

        startGameOnSceneLoad = false;
    }

    public void IncreaseDifficulty()
    {
        if (CurrentRunTime < nextDifficultyTime || !player || !segmentSpawner)
        {
            return;
        }

        if (!hasDifficultyDefaults)
        {
            originalPlayerSpeed = player.speed;
            originalMinGap = segmentSpawner.minGap;
            originalMaxGap = segmentSpawner.maxGap;
            hasDifficultyDefaults = true;
        }

        // Difficulty ramps up by combining faster forward movement with wider jumps,
        // while keeping both values capped so a long run stays playable.
        float maxPlayerSpeed = originalPlayerSpeed * 2f;

        player.speed = Mathf.Min(player.speed * (1f + speedIncreasePercent), maxPlayerSpeed);
        segmentSpawner.minGap = Mathf.Min(segmentSpawner.minGap + gapIncreaseAmount, maxGapSize);
        segmentSpawner.maxGap = Mathf.Min(segmentSpawner.maxGap + gapIncreaseAmount, maxGapSize);

        nextDifficultyTime += difficultyIncreaseInterval;
    }

    private void ResetDifficulty()
    {
        if (!player || !segmentSpawner)
        {
            return;
        }

        if (!hasDifficultyDefaults)
        {
            originalPlayerSpeed = player.speed;
            originalMinGap = segmentSpawner.minGap;
            originalMaxGap = segmentSpawner.maxGap;
            hasDifficultyDefaults = true;
        }

        player.speed = originalPlayerSpeed;
        segmentSpawner.minGap = originalMinGap;
        segmentSpawner.maxGap = originalMaxGap;
    }

    private void LoadSavedScores()
    {
        BestTime = SaveSystem.LoadBestTime();
        leaderboardScores.Clear();
        leaderboardScores.AddRange(SaveSystem.LoadLeaderboard());
    }

    private IEnumerator HandleGameOverTransition()
    {
        if (player != null)
        {
            // Let the player feedback finish before saving and changing scenes so the loss
            // reads as an intentional transition instead of an instant cut.
            yield return StartCoroutine(player.PlayGameOverFeedback());
        }

        SaveSystem.SaveTimer(LastRunTime);
        LoadSavedScores();
        UIManager?.ShowGameOver(LastRunTime);
        UIManager?.UpdateLeaderboardDisplay(BestTime, leaderboardScores);
        gameOverRoutine = null;
        SceneManager.LoadScene(GameOverSceneName);
    }

    private void SetGameState(GameState state)
    {
        CurrentGameState = state;
    }
}
