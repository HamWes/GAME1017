using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Leaderboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI leaderboardText;

    // Builds the visible leaderboard text using the saved best time plus the ranked score list
    public void Initialize(List<float> leaderboard, float bestTime)
    {
        if (leaderboardText == null)
        {
            return;
        }

        string leaderboardString = $"Best {UIManager.FormatTime(bestTime)}";
        leaderboardString += "\n\n<align=\"center\"><b>Leaderboard</b></align>";

        if (leaderboard == null || leaderboard.Count == 0)
        {
            leaderboardString += "\nNo scores yet";
        }
        else
        {
            for (int i = 0; i < leaderboard.Count; i++)
            {
                leaderboardString += $"\n{i + 1}. {UIManager.FormatTime(leaderboard[i])}";
            }
        }

        leaderboardText.text = leaderboardString;
    }

    // Clears the leaderboard text when the display needs to be reset
    public void Clear()
    {
        if (leaderboardText != null)
        {
            leaderboardText.text = string.Empty;
        }
    }
}
