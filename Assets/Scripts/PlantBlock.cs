using System.Collections;
using FMODUnity;
using UnityEngine;

[RequireComponent(typeof(MovableBlocks))]
public class PlantBlock : MonoBehaviour
{
    [SerializeField] GameObject segmentPrefab; // the plant part that we'll instantiate above the base plant (with no scripts)
    [SerializeField, Range(1, 2)] int baseHeight = 1; // the base of a plant can start at 1 or 2 blocks tall

    [Header("Growing")]
    [SerializeField] LayerMask waterMask;
    [SerializeField] LayerMask blockingMask; // any block which would block it from growing to prevent noclipping
    [SerializeField] float growDuration = 0.5f;

    [Header("Audio")]
    [SerializeField] EventReference plantGrowSFX;
    [SerializeField] EventReference plantShrinkSFX;


    static readonly Vector3[] Directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right }; // the range of the plant, shared across all the in-game instances

    MovableBlocks block;
    int height = 1;
    Transform grownSegment;  // the plant height block that gets added when next to water

    void Awake()
    {
        block = GetComponent<MovableBlocks>();

        for (int i = 1; i < baseHeight; i++)
            AddSegment(); // start at 2 tall if baseHeight is 2
    }

    void Update()
    {
        if (block.IsBusy || block.IsSinking) return; // not while being pushed, falling or sinking

        RotatingPlatform platform = GetComponentInParent<RotatingPlatform>();
        if (platform != null && platform.IsRotating) return; // positions are off-grid mid-rotation, wait for the platform to finsh rotations before growing

        bool nearWater = NearWater();
        if (nearWater && grownSegment == null) // when we're beside water, call the grow method
        {
            if (IsBlocked(transform.position + Vector3.up * height)) return; // if something is in the way, stop the growth first to prevent noclipping
            StartCoroutine(Grow());
        }
        else if (!nearWater && grownSegment != null) // if we push the block away from water, shrink back and lose the segmented block
        {
            StartCoroutine(Shrink());
        }
    }


    bool NearWater()
    {
        foreach (Vector3 dir in Directions)
        {
            Vector3 side = transform.position + dir;
            if (IsWater(side) || IsWater(side + Vector3.down)) return true; // water beside us, or in the floor next to us
        }
        return false;
    }

    bool IsWater(Vector3 cell) => Physics.CheckBox(cell, Vector3.one * 0.45f, Quaternion.identity, waterMask, QueryTriggerInteraction.Collide);
    bool IsBlocked(Vector3 cell) => Physics.CheckBox(cell, Vector3.one * 0.45f, Quaternion.identity, blockingMask, QueryTriggerInteraction.Ignore);

    Transform AddSegment()
    {
        Vector3 cell = transform.position + Vector3.up * height; // the position one block up from the base plant
        GameObject segment = Instantiate(segmentPrefab, cell, transform.rotation, transform); // transform - child of the plant, so it behaves with the base plant
        height++;
        return segment.transform;
    }

    IEnumerator Grow()
    {
        block.IsBusy = true; // player can't push it mid-grow

        AudioManager.instance.PlayOneShot(plantGrowSFX, transform.position);

        grownSegment = AddSegment();
        Vector3 target = grownSegment.position;

        yield return StartCoroutine(MoveSegment(grownSegment, target + Vector3.down, target)); // rises out of the block below

        block.IsBusy = false;
    }

    IEnumerator Shrink()
    {
        block.IsBusy = true;

        AudioManager.instance.PlayOneShot(plantShrinkSFX, transform.position);

        Vector3 from = grownSegment.position;

        yield return StartCoroutine(MoveSegment(grownSegment, from, from + Vector3.down)); // shrinks back into the block below

        Destroy(grownSegment.gameObject);
        grownSegment = null;
        height--;

        block.IsBusy = false;
    }
    IEnumerator MoveSegment(Transform segment, Vector3 from, Vector3 to) // shared by grow and shrink, just in opposite directions
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / growDuration;
            segment.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t)); // instantiate it inside the base plant block, then we're gonna make it slowly rise up with lerp
            yield return null;
        }
        segment.position = to; // snap exactly onto the grid
    }
}