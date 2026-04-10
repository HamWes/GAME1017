using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private const string GameSceneName = "GameScene";

    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text finalTimeText;

    public void BindSceneButtons(Scene scene)
    {
        playButton = null;
        resetButton = null;

        if (scene.name != GameSceneName)
        {
            return;
        }

        playButton = FindSceneButton(scene, GameManagerAction.PlayGame);
        resetButton = FindSceneButton(scene, GameManagerAction.RestartGame);
    }

    public void InitializeForMenu()
    {
        SetActive(playButton, true);
        SetActive(resetButton, false);
        SetActive(GetTimerDisplay(), false);
        SetActive(GetFinalTimeDisplay(), false);
        UpdateTimerDisplay(0f);
        UpdateFinalTimeDisplay(null);
    }

    public void ShowRunStarted(float currentTime)
    {
        SetActive(playButton, false);
        SetActive(resetButton, true);
        SetActive(GetTimerDisplay(), true);
        SetActive(GetFinalTimeDisplay(), false);
        UpdateTimerDisplay(currentTime);
        UpdateFinalTimeDisplay(null);
    }

    public void UpdateTimerDisplay(float currentTime)
    {
        SetText(timerText, FormatTime(currentTime));
    }

    public void ShowGameOver(float finalTime)
    {
        SetActive(playButton, false);
        SetActive(GetTimerDisplay(), false);
        SetActive(resetButton, false);
        SetActive(GetFinalTimeDisplay(), true);
        UpdateFinalTimeDisplay(finalTime);
    }

    private void UpdateFinalTimeDisplay(float? finalTime)
    {
        if (finalTimeText == null)
        {
            return;
        }

        finalTimeText.text = finalTime.HasValue
            ? $"Final {FormatTime(finalTime.Value)}"
            : string.Empty;
    }

    private static string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0f)
        {
            timeInSeconds = 0f;
        }

        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        float seconds = timeInSeconds % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }

    private static void SetActive(GameObject target, bool isActive) 
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private GameObject GetTimerDisplay()
    {
        return timerText != null ? timerText.gameObject : null;
    }

    private GameObject GetFinalTimeDisplay()
    {
        return finalTimeText != null ? finalTimeText.gameObject : null;
    }

    private static GameObject FindSceneButton(Scene scene, GameManagerAction action)
    {
        TitleScreenButton[] buttons = FindObjectsByType<TitleScreenButton>(FindObjectsSortMode.None);

        foreach (TitleScreenButton button in buttons)
        {
            if (button.gameObject.scene == scene && button.Action == action)
            {
                return button.gameObject;
            }
        }

        return null;
    }
}
