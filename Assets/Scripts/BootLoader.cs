using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    public const string SeenIntroKey = "HasSeenIntro";

    [SerializeField] string introScene = "IntroCutscene";
    [SerializeField] string menuScene = "MainMenuScreen";

    void Start()
    {
        bool seen = PlayerPrefs.GetInt(SeenIntroKey, 0) == 1; // if player has seenintro, bool = true.
        SceneManager.LoadScene(seen ? menuScene : introScene);
    }
}