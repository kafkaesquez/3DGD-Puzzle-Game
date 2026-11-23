using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroCutscene : MonoBehaviour
{
    [SerializeField] PlayableDirector director; //the gameobject with the PlayableDirector
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
        // start loading the menu now, but don't switch to it yet
        loadOp = SceneManager.LoadSceneAsync(nextScene); //returns AsyncOperation, and compared to LoadScene, it doenst freeze the sceene
        loadOp.allowSceneActivation = false;

        director.stopped += OnDirectorStopped; // when the director (part of the timeline plugin) stops, call OnDirectorStopped
        director.Play(); // initiate cutscene
    }

    void Update()
    {
        if (finished) return; //if the ctuscene is already done, return

        bool holding = Input.GetKey(skipKey);

        heldTime = holding //heldtime = how long the space bar is pressed
            ? heldTime + Time.unscaledDeltaTime // ? : is a compact vers of "if else" and this means as long as spacebar is being pressed, heldtime adds secs with time.unscaleddeltatime and if let go, reset back to 0
            : 0f; //unscaled delta time is independent counter from the last frame to the current, and isnt affected by stuff like Time.timescale slowmos

        if (skipRing) skipRing.fillAmount = heldTime / holdToSkip; // reference for the UI visual
        if (heldTime >= holdToSkip) Skip(); //once heldTime = to like 1 second, skip the cutscene
    }

    void Skip()
    {
        // if the cutscene led into gameplay in the SAME scene, you'd first do
        // director.time = director.duration; director.Evaluate();
        // so everything lands in its final pose. we're changing scene, so just stop.
        director.Stop();
    }

    void OnDirectorStopped(PlayableDirector _)
    {
        if (finished) return;
        finished = true;

        PlayerPrefs.SetInt(BootLoader.SeenIntroKey, 1); //saves the data that they've seen the intro, same concept with the gold collectible and then Bootloader helps to send the player straight to the main menu instead
        PlayerPrefs.Save();

        loadOp.allowSceneActivation = true; // swap to the preloaded menu, so that theres no freezes in-between n stuff
    }
}