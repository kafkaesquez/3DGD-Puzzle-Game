using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevelButton : MonoBehaviour
{
    [SerializeField] string menuScene = "MainMenuScreen"; // where the last level sends you

    public void Load()
    {
        Time.timeScale = 1f;

        int next = SceneManager.GetActiveScene().buildIndex + 1; // the scene right after this one in the buuild profiles (better than inputting string scene = for every level

        if (next < SceneManager.sceneCountInBuildSettings) // check if theres a scene after, then load it
            SceneManager.LoadScene(next);
        else
            SceneManager.LoadScene(menuScene); // else if load main menu.
    }
}