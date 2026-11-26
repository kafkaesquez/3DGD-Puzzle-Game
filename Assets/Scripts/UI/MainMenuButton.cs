using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;

public class MainMenuButton : MonoBehaviour
{
    [Header("AudioClip")]
    [SerializeField] EventReference MainMenuButtonClick;
    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScreen");
        AudioManager.instance.PlayOneShot(MainMenuButtonClick, transform.position);
    }
}
