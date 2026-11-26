using UnityEngine;

public class LeaveGameDetector : MonoBehaviour
{
    [SerializeField] LayerMask LeaveGameMask;

    bool quitting;

    void Update()
    {
        if (quitting) return;

        if (Physics.CheckBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, LeaveGameMask, QueryTriggerInteraction.Collide))
        {
            quitting = true;
            QuitGame();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR // #if and etc is a preprocessor directive, which means this is loaded before the code compiles, not while the game is running 
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}