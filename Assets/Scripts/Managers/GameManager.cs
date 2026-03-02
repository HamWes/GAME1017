using UnityEngine;

public enum GameState
{
    InMenu,
    InGame,
    GameOver
}

public class GameManager : Singleton<GameManager>
{
    public GameState CurrentGameState { get; private set; }

    [SerializeField] private PlayerController player;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private SegmentSpawner segmentSpawner;

    public PlayerController Player
    {
        get
        {
            if (player == null)
                player = FindFirstObjectByType<PlayerController>();

            return player;
        }
        private set
        {
            player = value;
        }
    }

    public SoundManager SoundManager
    {
        get
        {
            if (soundManager == null)
                soundManager = FindFirstObjectByType<SoundManager>();

            return soundManager;
        }
        private set
        {
            soundManager = value;
        }
    }

    public SegmentSpawner SegmentSpawner
    {
        get
        {
            if (segmentSpawner == null)
                segmentSpawner = FindFirstObjectByType<SegmentSpawner>();

            return segmentSpawner;
        }
        private set
        {
            segmentSpawner = value;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentGameState = GameState.InMenu;
    }

    public void PlayGame()
    {
        SetGameState(GameState.InGame);
        Player.Initialize();
        segmentSpawner.Initialize();
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
    }

    [ContextMenu("Restart Game")]
    public void RestartGame()
    {
        player.ResetPlayer();
        SetGameState(GameState.InMenu);
    }

    private void SetGameState(GameState state)
    {
        CurrentGameState = state;
    }
}
