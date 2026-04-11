using System.Collections.Generic;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
    private const string BestTimeKey = "BestTime";
    private const string LeaderboardKey = "LeaderboardScore";
    private const string LeaderboardCountKey = "LeaderboardScoreCount";
    private static readonly List<float> leaderboard = new();
    private const int maxScoresSize = 5;
    private const float DuplicateScoreTolerance = 0.001f;

    public static void SaveTimer(float timer)
    {
        leaderboard.Clear();
        leaderboard.AddRange(LoadLeaderboard());

        bool scoreAlreadyStored = false;

        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (Mathf.Abs(leaderboard[i] - timer) <= DuplicateScoreTolerance)
            {
                scoreAlreadyStored = true;
                break;
            }
        }

        if (!scoreAlreadyStored)
        {
            leaderboard.Add(timer);
        }

        SortAndTrimLeaderboard(leaderboard);

        SaveLeaderboardEntries(leaderboard);

        for (int i = 0; i < leaderboard.Count; i++)
        {
            if (leaderboard[i] > PlayerPrefs.GetFloat(BestTimeKey, 0f))
            {
                PlayerPrefs.SetFloat(BestTimeKey, leaderboard[i]);
            }
        }

        PlayerPrefs.Save();
    }

    public static float LoadBestTime()
    {
        return PlayerPrefs.GetFloat(BestTimeKey, 0f);
    }

    public static List<float> LoadLeaderboard()
    {
        leaderboard.Clear();

        int scoreCount = Mathf.Min(PlayerPrefs.GetInt(LeaderboardCountKey, 0), maxScoresSize);

        for (int i = 0; i < scoreCount; i++)
        {
            leaderboard.Add(PlayerPrefs.GetFloat($"{LeaderboardKey}{i}", 0f));
        }

        SortAndTrimLeaderboard(leaderboard);

        return new List<float>(leaderboard);
    }

    private static void SortAndTrimLeaderboard(List<float> scores)
    {
        scores.Sort((a, b) => b.CompareTo(a));

        for (int i = scores.Count - 1; i > 0; i--)
        {
            if (Mathf.Abs(scores[i] - scores[i - 1]) <= DuplicateScoreTolerance)
            {
                scores.RemoveAt(i);
            }
        }

        if (scores.Count > maxScoresSize)
        {
            scores.RemoveRange(maxScoresSize, scores.Count - maxScoresSize);
        }
    }

    private static void SaveLeaderboardEntries(List<float> scores)
    {
        PlayerPrefs.SetInt(LeaderboardCountKey, scores.Count);

        for (int i = 0; i < scores.Count; i++)
        {
            PlayerPrefs.SetFloat($"{LeaderboardKey}{i}", scores[i]);
        }

        for (int i = scores.Count; i < maxScoresSize; i++)
        {
            PlayerPrefs.DeleteKey($"{LeaderboardKey}{i}");
        }
    }
}
