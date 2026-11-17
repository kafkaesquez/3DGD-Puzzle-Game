using UnityEngine;

/// <summary>
/// Holds the star-related data for levels. Static (no GameObject needed) so it
/// survives scene reloads and scene changes.
///
/// Stars (planned):
///   1 star  : finish the stage
///   2 stars : complete by timer
///   3 stars : collect the gold block
/// </summary>
public static class LevelStarData
{
    // True once the gold block has been collected in the CURRENT attempt.
    // Resets every time the level starts, because dying reloads the scene
    // and the gold block respawns with it.
    public static bool GoldCollected { get; private set; }

    public static void ResetAttempt() => GoldCollected = false; // => is the same as { line } and is apparently often used for one liner methods, easier to type

    public static void CollectGold() => GoldCollected = true;

    // Call when the player reaches the exit: makes the gold permanent for that level.
    public static void SaveLevel(string levelName)
    {
        if (GoldCollected) // never overwrite an earlier success with a failed run
        {
            PlayerPrefs.SetInt(GoldKey(levelName), 1); //playerpref is unity's inbuilt key-value storage, and save() uploads it to the disk while setint stores the value in the memory
            PlayerPrefs.Save(); //to wipe the saved data, go to edit, then clear all playerpref (note for self)
        }
    }

    // For level select screens / star counts later on.
    public static bool HasGold(string levelName) => PlayerPrefs.GetInt(GoldKey(levelName), 0) == 1;

    static string GoldKey(string levelName) => $"Gold_{levelName}";
}