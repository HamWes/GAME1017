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

    // StartScene now owns the shared managers, so these are assigned once in the inspector instead of using lazy getter/setter lookups that could bind to disabled duplicates in GameScene.
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private UIManager uiManager;

    // This flag lets the title scene request a run before GameScene has finished loading.
    private bool startGameOnSceneLoad;
    private bool hasInitializedActiveScene;

    public GameState CurrentGameState { get; private set; } = GameState.InMenu;
    public float CurrentRunTime { get; private set; }
    public float LastRunTime { get; private set; }
    public UIManager UIManager => uiManager;
    public SoundManager SoundManager => soundManager;

    private bool TimerRunning => CurrentGameState == GameState.InGame;

    protected override void Awake()
    {
        base.Awake();

        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance != this || hasInitializedActiveScene)
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

        PlayerController player = FindFirstObjectByType<PlayerController>();
        BackgroundManager backgroundManager = FindFirstObjectByType<BackgroundManager>();

        CurrentRunTime = 0f;
        startGameOnSceneLoad = false;
        player?.ResetPlayer();
        backgroundManager?.Initialize();
        SetGameState(GameState.InMenu);
        UIManager?.InitializeForMenu();
    }

    public void GameOver()
    {
        if (CurrentGameState == GameState.GameOver)
        {
            return;
        }

        LastRunTime = CurrentRunTime;
        SetGameState(GameState.GameOver);
        UIManager?.ShowGameOver(LastRunTime);
        SceneManager.LoadScene(GameOverSceneName);
    }

    public void ReturnToTitle()
    {
        startGameOnSceneLoad = false;
        CurrentRunTime = 0f;
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
        hasInitializedActiveScene = true;
        UIManager?.BindSceneButtons(scene);

        switch (scene.name)
        {
            case StartSceneName:
                SetGameState(GameState.InMenu);
                UIManager?.InitializeForMenu();
                break;

            case GameSceneName:
                if (startGameOnSceneLoad)
                {
                    StartRun(resetTimer: true);
                }
                else
                {
                    // Opening GameScene directly in the editor should still show a clean menu state.
                    CurrentRunTime = 0f;
                    SetGameState(GameState.InMenu);
                    UIManager?.InitializeForMenu();
                }
                break;

            case GameOverSceneName:
                SetGameState(GameState.GameOver);
                UIManager?.ShowGameOver(LastRunTime);
                break;
        }
    }

    private void StartRun(bool resetTimer)
    {
        // Gameplay systems live in GameScene, so we grab them from the active scene when a run starts instead of treating them like persistent managers.
        PlayerController player = FindFirstObjectByType<PlayerController>();
        SegmentSpawner segmentSpawner = FindFirstObjectByType<SegmentSpawner>();
        BackgroundManager backgroundManager = FindFirstObjectByType<BackgroundManager>();

        if (resetTimer)
        {
            CurrentRunTime = 0f;
        }

        SetGameState(GameState.InGame);

        player?.ResetPlayer();
        player?.Initialize();
        segmentSpawner?.Initialize();
        backgroundManager?.Initialize();
        UIManager?.ShowRunStarted(CurrentRunTime);

        startGameOnSceneLoad = false;
    }

    private void SetGameState(GameState state)
    {
        CurrentGameState = state;
    }
}
