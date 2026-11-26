using UnityEngine;
using FMODUnity;
using System.Collections;

public class ExitButton : MonoBehaviour
{
    [Header("AudioClip")]
    [SerializeField] EventReference ExitGame;
    float quitDelay = 1f;
    public void ExitGameButton()
    {
        AudioManager.instance.PlayOneShot(ExitGame, transform.position);
        StartCoroutine(QuitAfterDelay());
    }
    IEnumerator QuitAfterDelay()
    {
        yield return new WaitForSecondsRealtime(quitDelay);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
