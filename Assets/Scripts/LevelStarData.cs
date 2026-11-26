using UnityEngine;
public static class LevelStarData
{
    public static bool GoldCollected { get; private set; }
    public static bool BeatTime { get; private set; }

    public static void ResetAttempt() => GoldCollected = false; // => is the same as { line } and is apparently often used for one liner methods, easier to type

    public static void CollectGold() => GoldCollected = true;

    public static void SetBeatTime(bool beat) => BeatTime = beat;


    public static void SaveLevel(string levelName)
    {
        PlayerPrefs.SetInt(CompleteKey(levelName), 1); // reaching the exit = level finished (1 star)

        if (GoldCollected) // never overwrite an earlier success with a failed run
            PlayerPrefs.SetInt(GoldKey(levelName), 1);

        if (BeatTime)
            PlayerPrefs.SetInt(TimeKey(levelName), 1);

        PlayerPrefs.Save();
    }

    public static bool HasGold(string levelName) => PlayerPrefs.GetInt(GoldKey(levelName), 0) == 1;
    public static bool IsComplete(string levelName) => PlayerPrefs.GetInt(CompleteKey(levelName), 0) == 1;
    public static bool HasTimeStar(string levelName) => PlayerPrefs.GetInt(TimeKey(levelName), 0) == 1;


    static string GoldKey(string levelName) => $"Gold_{levelName}";
    static string CompleteKey(string levelName) => $"Complete_{levelName}";

    static string TimeKey(string levelName) => $"Time_{levelName}";

    public static int StarCount(string levelName)
    {
        int stars = 0;

        if (IsComplete(levelName))
        {
            stars += 1;
        }

        if (HasTimeStar(levelName))
        {
            stars += 1;
        }

        if (HasGold(levelName))
        {
            stars += 1;
        }

        return stars;
    }
}