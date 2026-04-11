using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private const string StartSceneName = "StartScene";
    private const string GameSceneName = "GameScene";

    [SerializeField] private GameObject playButton;
    [SerializeField] private GameObject resetButton;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text finalTimeText;
    [SerializeField] private Leaderboard leaderboardDisplay;

    // Finds the gameplay buttons that live in GameScene so this persistent UI manager can control them
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

    // Shows the menu-state UI: Play visible, gameplay-only elements hidden, leaderboard visible on StartScene
    public void InitializeForMenu()
    {
        SetActive(playButton, true);
        SetActive(resetButton, false);
        SetActive(GetTimerDisplay(), false);
        SetActive(GetFinalTimeDisplay(), false);
        SetActive(GetLeaderboardDisplay(), leaderboardDisplay != null && SceneManager.GetActiveScene().name == StartSceneName);
        UpdateTimerDisplay(0f);
        UpdateFinalTimeDisplay(null);
    }

    // Switches the UI into run mode by showing the timer/reset button and hiding menu-only displays
    public void ShowRunStarted(float currentTime)
    {
        SetActive(playButton, false);
        SetActive(resetButton, true);
        SetActive(GetTimerDisplay(), true);
        SetActive(GetFinalTimeDisplay(), false);
        SetActive(GetLeaderboardDisplay(), false);
        UpdateTimerDisplay(currentTime);
        UpdateFinalTimeDisplay(null);
    }

    // Formats the active run time and sends it to the timer text
    public void UpdateTimerDisplay(float currentTime)
    {
        SetText(timerText, FormatTime(currentTime));
    }

    // Shows the final-time view after a loss and re-enables the leaderboard display
    public void ShowGameOver(float finalTime)
    {
        SetActive(playButton, false);
        SetActive(GetTimerDisplay(), false);
        SetActive(resetButton, false);
        SetActive(GetFinalTimeDisplay(), true);
        SetActive(GetLeaderboardDisplay(), leaderboardDisplay != null);
        UpdateFinalTimeDisplay(finalTime);
    }

    // Updates or clears the final-time label depending on whether a finished run value exists
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

    // Passes best-time and score-list data to the leaderboard formatter component
    public void UpdateLeaderboardDisplay(float bestTime, List<float> leaderboard)
    {
        if (leaderboardDisplay == null)
        {
            return;
        }

        leaderboardDisplay.Initialize(leaderboard, bestTime);
    }

    public static string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds < 0f)
        {
            timeInSeconds = 0f;
        }

        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        float seconds = timeInSeconds % 60f;
        return $"{minutes:00}:{seconds:00.00}";
    }

    // Null-safe helper so callers do not repeat the same guard code
    private static void SetActive(GameObject target, bool isActive) 
    {
        if (target != null)
        {
            target.SetActive(isActive);
        }
    }

    // Null-safe helper used by the timer and final-time labels
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

    private GameObject GetLeaderboardDisplay()
    {
        return leaderboardDisplay != null ? leaderboardDisplay.gameObject : null;
    }

    // Searches a specific scene for the button assigned to a given GameManager action
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
