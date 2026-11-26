using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitStage : MonoBehaviour
{
    [SerializeField] Timer timer;
    private bool loading;
    [SerializeField] GameObject levelCompletePanel;

    [Header("Audio")]
    [SerializeField] EventReference LevelCompleteSFX;

    public void Load()
    {
        if (loading) return;
        loading = true;
        
        timer.StopTimer();
        LevelStarData.SaveLevel(SceneManager.GetActiveScene().name); //save the current scene's data
        AudioManager.instance.PlayOneShot(LevelCompleteSFX, transform.position);

        levelCompletePanel.SetActive(true);
        Time.timeScale = 0f;
    }
}