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

    // Sets up the persistent manager, loads saved score data, and hooks scene load events once
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

    // Runs the same scene setup on the first loaded scene so startup and later scene loads share one path
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

    // Advances the run timer only during gameplay, then updates difficulty and the visible timer UI
    private void Update()
    {
        if (!TimerRunning)
        {
            return;
        }

        // The run timer only advances during active gameplay
        CurrentRunTime += Time.deltaTime;
        IncreaseDifficulty();
        UIManager?.UpdateTimerDisplay(CurrentRunTime);
    }

    // Requests gameplay from any scene, loading GameScene first when needed
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

    // Resets the run back to a menu state inside GameScene without leaving the scene
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

    // Captures the finished run time once and starts the delayed loss transition
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

    // Returns to the title scene and clears run-specific state.
    public void ReturnToTitle()
    {
        startGameOnSceneLoad = false;
        CurrentRunTime = 0f;
        nextDifficultyTime = difficultyIncreaseInterval;
        SetGameState(GameState.InMenu);
        SceneManager.LoadScene(StartSceneName);
    }

    // Unity event callback that forwards all scene setup through the shared handler below
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Instance != this)
        {
            return;
        }

        HandleSceneLoaded(scene);
    }

    // Rebinds scene-specific gameplay objects and updates UI/state based on the scene that just loaded
    private void HandleSceneLoaded(Scene scene)
    {
        UIManager?.BindSceneButtons(scene);

        if (scene.name == GameSceneName)
        {
            // Gameplay objects are rebound whenever GameScene loads
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

    // Starts a fresh run by resetting state and initializing the gameplay systems used in GameScene
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

    // Gradually increases challenge over time by raising speed and jump gap values with safe caps
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

        float maxPlayerSpeed = originalPlayerSpeed * 2f;

        player.speed = Mathf.Min(player.speed * (1f + speedIncreasePercent), maxPlayerSpeed);
        segmentSpawner.minGap = Mathf.Min(segmentSpawner.minGap + gapIncreaseAmount, maxGapSize);
        segmentSpawner.maxGap = Mathf.Min(segmentSpawner.maxGap + gapIncreaseAmount, maxGapSize);

        nextDifficultyTime += difficultyIncreaseInterval;
    }

    // Restores the original gameplay values so each run starts from the same baseline
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

    // Pulls the saved best time and leaderboard into memory for UI display and game-over handling
    private void LoadSavedScores()
    {
        BestTime = SaveSystem.LoadBestTime();
        leaderboardScores.Clear();
        leaderboardScores.AddRange(SaveSystem.LoadLeaderboard());
    }

    // Waits for the player loss feedback, then saves scores, refreshes the UI data, and loads GameOverScene
    private IEnumerator HandleGameOverTransition()
    {
        if (player != null)
        {
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
