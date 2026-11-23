using System.Collections;
using UnityEngine;

public class MovableBlocks : MonoBehaviour
{
    [SerializeField] LayerMask obstacleMask = 3; //ground layer
    [SerializeField] LayerMask KillMask; //if wood / rock falls on lava / water, sink and destroy iteself.
    [SerializeField] LayerMask FillMask;// this is to make it so that wood and rock also sinks but doesnt get destroyed in their respective substances
    float sinkDepth = 1f;
    float sinkDuration = 0.6f;

    [Header("Gravity")]
    [SerializeField] float gravity = 25f; // same stuff as player controller
    [SerializeField] float maxFallSpeed = 20f;      

    public bool IsBusy { get; set; } //true while the block is falling or being pushed/pulled to prevent double inputs
    public bool IsSinking { get; private set; } //to communicate to the player that theres a block sinking, so ucant climb onto it

    [Header("Block Sounds")]
    [SerializeField] SinkingSound[] sinkingSounds; // array so that it doesnt get messy when adding sfx interactions with future blocks like the plant or more

    RotatingPlatform ridingPlatform; // new property as rn moving blocks dont stay on platforms as they rotate

    void Update()
    {
        if (ridingPlatform != null && ridingPlatform.IsRotating) return;
        if (IsBusy) return;

        UpdateRide();

        Vector3 pos = transform.position;

        Vector3 below = transform.position + Vector3.down;
        bool supported = IsSolid(below);

        if (IsKillMask(pos) || (!supported && IsKillMask(below)))
        {
            StartCoroutine(SinkAndDestroy());
            return;
        }

        if (IsFillMask(pos) || (!supported && IsFillMask(below)))
        {
            StartCoroutine(Sink()); // plugged the liquid: stay here as solid ground and stop updating
            return;
        }

       if (!supported)
        {
            StartCoroutine(Fall());
            return;
        }
    }

    bool IsLiquid(Vector3 cellCenter) => IsFillMask(cellCenter) || IsKillMask(cellCenter); // cellcenter means the cell position that we wanna check of the detected layermask, and checks both water/lava vice versa depending on the movable block tag

    IEnumerator Fall()
    {
        IsBusy = true;
        float velocity = 0f;

        while (!IsSolid(transform.position + Vector3.down) && !IsLiquid(transform.position + Vector3.down)) //there was this bug where if i pushed a stone from a height of 2 blocks down onto lava, it sinks below the position of the lava block. the reasoning was the fall method didnt account and stop the block from falling before hitting the lava.
        {
            Vector3 target = transform.position + Vector3.down;

            while (transform.position != target)
            {
                velocity = Mathf.Min(velocity + gravity * Time.deltaTime, maxFallSpeed); //samestuff in play control
                transform.position = Vector3.MoveTowards(transform.position, target, velocity * Time.deltaTime); //smoothen the falling and make it look natural, but basically same stuff with playercontroller
                yield return null;
            }
        }

        IsBusy = false;
    }
    bool IsFillMask(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity,FillMask.value, QueryTriggerInteraction.Collide);
    bool IsKillMask(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, KillMask.value, QueryTriggerInteraction.Collide);
    bool IsSolid(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, obstacleMask.value & ~(KillMask.value | FillMask.value), QueryTriggerInteraction.Ignore); //same in playercontroller, and ignores istrigger type of colliders.

    IEnumerator SinkAndDestroy()
    {
        IsBusy = true;
        IsSinking = true;

        AudioManager.instance.PlayOneShot(GetSinkingSound(GetKillTag()).sink, transform.position);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * sinkDepth; //initalizing lerp
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / sinkDuration;
            transform.position = Vector3.Lerp(start, end, t); // linear interpolation to make a smooth sinking "animation" when player touches lava or water
            yield return null;
        }
        Destroy(gameObject);
    }

    IEnumerator Sink()
    {
        IsBusy = true;
        IsSinking = true;

        AudioManager.instance.PlayOneShot(GetSinkingSound(GetLiquidTag()).sink, transform.position);

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * sinkDepth; //initalizing lerp
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / sinkDuration;
            transform.position = Vector3.Lerp(start, end, t); // linear interpolation to make a smooth sinking "animation" when player touches lava or water
            yield return null;
        }
        IsSinking = false;
        DrainLiquid(start); // liquid in the cell the block was in
        DrainLiquid(start + Vector3.down);
    }

    void DrainLiquid(Vector3 cell)
    {
        foreach (Collider c in Physics.OverlapBox(cell, Vector3.one * 0.45f, Quaternion.identity, FillMask, QueryTriggerInteraction.Collide))
            c.gameObject.SetActive(false);
    }

    SinkingSound GetSinkingSound(string surfaceTag)
    {
        foreach (SinkingSound s in sinkingSounds)
            if (s.tag == surfaceTag) return s;
        return null;
    }

    string GetLiquidTag()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, FillMask, QueryTriggerInteraction.Collide); //checks infront
        if (hits.Length == 0)
        hits = Physics.OverlapBox(transform.position + Vector3.down, Vector3.one * 0.45f, Quaternion.identity, FillMask, QueryTriggerInteraction.Collide); //checks below

        if (hits.Length > 0)
        {
            return hits[0].tag;
        }

        return "Untagged";
    }

    string GetKillTag()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, KillMask, QueryTriggerInteraction.Collide); //checks infront
        if (hits.Length == 0)
            hits = Physics.OverlapBox(transform.position + Vector3.down, Vector3.one * 0.45f, Quaternion.identity, KillMask, QueryTriggerInteraction.Collide); //checks below

        if (hits.Length > 0)
        {
            return hits[0].tag; //same as the getliquidtag but the KillMask detection is added.
        }

        return "Untagged";
    }

    string GetPlatformTag()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.down * 0.45f, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Collide); //checks below for the tag of the block if its a platform
        if (hits.Length == 0)

        if (hits.Length > 0)
        {
            return hits[0].tag; 
        }

        return "Untagged";
    }

    void UpdateRide()
    {
        RotatingPlatform platformBelow = GetPlatformBelow();
        if (platformBelow == ridingPlatform) return;

        ridingPlatform = platformBelow;
        transform.SetParent(ridingPlatform != null ? ridingPlatform.transform : null, true); // true keeps its world position
    }

    RotatingPlatform GetPlatformBelow()
    {
        Collider[] hits = Physics.OverlapBox(transform.position + Vector3.down, Vector3.one * 0.45f, Quaternion.identity, obstacleMask.value, QueryTriggerInteraction.Ignore); //ignore the kill masks

        foreach (Collider c in hits)
        {
            RotatingPlatform p = c.GetComponentInParent<RotatingPlatform>(); //parent the movable block to the platform, so it moves with the platform/
            if (p != null) return p;
        }
        return null;
    }
}