using UnityEngine;
using UnityEngine.UI;

public enum GameManagerAction
{
    PlayGame,
    RestartGame,
    GameOver,
    ReturnToTitle
}

[RequireComponent(typeof(Button))]
public class TitleScreenButton : MonoBehaviour
{
    [SerializeField] private GameManagerAction gmAction;
    public GameManagerAction Action => gmAction;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        button.onClick.AddListener(PerformGameManagerAction);
    }

    private void OnDisable()
    {
        button.onClick.RemoveListener(PerformGameManagerAction);
    }

    private void PerformGameManagerAction()
    {
        switch (gmAction)
        {
            case GameManagerAction.PlayGame:
                GameManager.Instance.PlayGame();
                break;

            case GameManagerAction.RestartGame:
                GameManager.Instance.RestartGame();
                break;

            case GameManagerAction.GameOver:
                GameManager.Instance.GameOver();
                break;

            case GameManagerAction.ReturnToTitle:
                GameManager.Instance.ReturnToTitle();
                break;
        }
    }
}
