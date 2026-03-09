// this persists across ALL scenes, only one exists throughout the game
using UnityEngine;
using System.Collections.Generic;

public static class GameProgress
{
    // Store points per level: levelNumber -> points earned
    private static Dictionary<int, int> levelPoints = new Dictionary<int, int>();

    // Store completion status
    private static bool gameCompleted = false;

    // max points 
    private const int MAX_POINTS = 1000;

    // === SAVING POINTS ===

    public static void SaveLevelPoints(int levelNumber, int points)
    {
        // If level already has points, overwrite them
        // (in case player replays a level)
        levelPoints[levelNumber] = points;
    }

    // === READING POINTS ===

    public static int GetLevelPoints(int levelNumber)
    {
        // TryGetValue returns false if key doesn't exist
        if (levelPoints.TryGetValue(levelNumber, out int points))
        {
            return points;
        }
        return 0;  // Level not played yet
    }

    public static int GetTotalPoints()
    {
        int total = 0;
        foreach (int points in levelPoints.Values)
        {
            total += points;
        }
        return total;
    }

    public static string GetRank()
    {
        float percentage = (float)GetTotalPoints() / MAX_POINTS * 100f;

        if (percentage >= 91) return "Elite";
        if (percentage >= 81) return "Meister";
        if (percentage >= 61) return "Gold";
        if (percentage >= 31) return "Silber";
        return "Bronze";
    }

    // === GAME STATE ===

    public static void MarkGameCompleted()
    {
        gameCompleted = true;
    }

    public static bool IsGameCompleted() => gameCompleted;

    // === RESET (important) ===

    public static void ResetAllProgress()
    {
        levelPoints.Clear();
        gameCompleted = false;
    }
}