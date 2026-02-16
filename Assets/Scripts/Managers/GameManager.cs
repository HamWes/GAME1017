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

    private PlayerController player;

    public SoundManager SoundManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        CurrentGameState = GameState.InMenu;

        player = FindFirstObjectByType<PlayerController>();
    }

    public void PlayGame()
    {
        SetGameState(GameState.InGame);
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
