using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PaintingsLevelLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private bool loading;

    [Header("Audio")]
    [SerializeField] EventReference PaintingTransition;

    public void Load()
    {
        if (loading) return;
        loading = true;
        AudioManager.instance.PlayOneShot(PaintingTransition, transform.position);

        SceneManager.LoadScene(sceneName);
    }
}