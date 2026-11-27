using UnityEngine;
using System.Collections;
using FMODUnity;

public class PlantSpawn : MonoBehaviour
{
    [SerializeField] GameObject plant;
    [SerializeField] GameObject plantPrefab;
    float riseDuration = 2f;
    [SerializeField] LayerMask blockingMask;

    [Header("Audio")]
    [SerializeField] EventReference spawnPlant;

    Vector3 startPosition;

    void Start()
    {
        startPosition = plant.transform.position; // spawn the plant back where it originally was if it got destroyed
    }

    bool IsBlocked(Vector3 cell) => Physics.CheckBox(cell, Vector3.one * 0.45f, Quaternion.identity, blockingMask, QueryTriggerInteraction.Ignore);
    void Update()
    {
        if (plant == null) // it got destroyed (e.g. sank in lava)
        {
            if (IsBlocked(transform.position + Vector3.up)) return;
            plant = Instantiate(plantPrefab, startPosition + Vector3.down, Quaternion.identity); // spawn it one block underground and then we'll make it rise up
            StartCoroutine(RiseUp(plant));
        }
    }

    IEnumerator RiseUp(GameObject newPlant)
    {
        MovableBlocks block = newPlant.GetComponent<MovableBlocks>();
        block.IsBusy = true; // lock it so it doesn't try to fall/sink/grow while it's still inside the floor

        AudioManager.instance.PlayOneShot(spawnPlant, transform.position);

        Vector3 from = startPosition + Vector3.down; // inside the ground block
        Vector3 to = startPosition; // the original position that the plant block was at
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / riseDuration;
            newPlant.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)); // same concept with plant block
            yield return null;
        }

        newPlant.transform.position = to; // snap exactly onto the grid
        block.IsBusy = false;
    }
}