using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitStage : MonoBehaviour
{
    [SerializeField] private string sceneName;
    private bool loading;

    public void Load()
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadScene(sceneName);
    }
}