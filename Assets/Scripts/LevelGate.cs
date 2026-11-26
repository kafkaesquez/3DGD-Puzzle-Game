using UnityEngine;

public class LevelGate : MonoBehaviour
{
    [SerializeField] string requiredLevel; //the level required to have been completed for the gate to be disabled
    [SerializeField] GameObject[] GalleryFence; 

    void Start()
    {
        if (!LevelStarData.IsComplete(requiredLevel)) return;
        foreach (GameObject go in GalleryFence)
            if (go != null) go.SetActive(false);
    }
}