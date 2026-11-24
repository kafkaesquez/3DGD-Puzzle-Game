using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] VideoPlayer video;
    [SerializeField] string nextScene = "MainMenuScreen";

    [Header("Skip")]
    [SerializeField] KeyCode skipKey = KeyCode.Space; // e.g. if player presses space button, skip cutscene (for those who reopen the application after already playing the game)
    [SerializeField] float holdToSkip = 1f; // how long the player has to press the button before skipping, so it doesnt accidentally skips on a misclick
    [SerializeField] Image skipRing; // visuals to show how long before skip

    AsyncOperation loadOp;  // async allows the loading to happen in the background instead of pausing everything causing the cutscene to freeze mid-shot
    float heldTime;
    bool finished;

    void Start()
    {
        loadOp = SceneManager.LoadSceneAsync(nextScene); //load the menu in the background but dont switch yet
        loadOp.allowSceneActivation = false;

        video.loopPointReached += OnVideoFinished; // is called when the video reaches the end
        video.prepareCompleted += OnVideoPrepared; // is called when the video is ready
        video.Prepare(); //wait for it to be ready, then call the method andp lay the video
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    void Update()
    {
        if (finished) return; //if the ctuscene is already done, return

        bool holding = Input.GetKey(skipKey);

        heldTime = holding //heldtime = how long the space bar is pressed
            ? heldTime + Time.unscaledDeltaTime // ? : is a compact vers of "if else" and this means as long as spacebar is being pressed, heldtime adds secs with time.unscaleddeltatime and if let go, reset back to 0
            : 0f; //unscaled delta time is independent counter from the last frame to the current, and isnt affected by stuff like Time.timescale slowmos

        if (skipRing) skipRing.fillAmount = heldTime / holdToSkip; // reference for the UI visual, and only shows up if fillAmount has value
        if (heldTime >= holdToSkip) Finish(); //once heldTime = to like 1 second, skip the cutscene
    }

    void OnVideoFinished(VideoPlayer vp) => Finish();

    void Finish()
    {
        if (finished) return;
        finished = true;

        video.Stop();

        PlayerPrefs.SetInt(BootLoader.SeenIntroKey, 1); //saves the data that they've seen the intro, same concept with the gold collectible and then Bootloader helps to send the player straight to the main menu instead
        PlayerPrefs.Save();

        loadOp.allowSceneActivation = true; // swap to the preloaded menu
    }
}