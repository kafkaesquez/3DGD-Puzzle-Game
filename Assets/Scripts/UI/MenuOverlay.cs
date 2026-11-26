using UnityEngine;
using FMODUnity;

public class MenuOverlay : MonoBehaviour
{
    public GameObject overlay;
    public static bool IsPaused { get; private set; }

    [Header("AudioClip")]
    [SerializeField] EventReference EscapeButtonClick;

    [SerializeField] GameObject levelCompletePanel;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Toggle();
    }

    public void Toggle()
    {
        if (levelCompletePanel.activeSelf) return;
        overlay.SetActive(!overlay.activeSelf);
        IsPaused = overlay.activeSelf;
        Time.timeScale = IsPaused ? 0f : 1f; // ? is a one line ifelse, so if overlay is activated, time is paused.
        AudioManager.instance.PlayOneShot(EscapeButtonClick, transform.position);
    }
    void OnDestroy()
    {
        IsPaused = false; // reset when leaving the scene
    }
}