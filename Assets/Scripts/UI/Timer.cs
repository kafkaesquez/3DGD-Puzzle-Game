using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text timeLabel;
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color overTimeColor = Color.red;

    [Header("Level Settings")]
    [SerializeField] float targetTime = 60f; //changable time target for each level

    public float Elapsed { get; private set; }
    public bool BeatTarget => Elapsed <= targetTime; // bool is true if elapsed is less than or equal to targettime

    bool running = true;

    void Update()
    {
        if (!running) return;

        Elapsed += Time.deltaTime;
        timeLabel.text = $"time: {Elapsed:F2} / {targetTime:F0} s"; // elapsed time shown in 2 dp, while targettime is rounded to 0 dp
        timeLabel.color = BeatTarget ? normalColor : overTimeColor; // if beatTarget = true, timer is within the normal colour, else, turn it red to indicate and communicate to the player that the challenge is failed
    }

    public void StopTimer()
    {
        running = false;
        LevelStarData.SetBeatTime(BeatTarget);
    }
}