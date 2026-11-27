using Cinemachine;
using FMODUnity;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 2.5f; // squares per second when sliding
    [SerializeField] float climbTime = 0.5f; // seconds the climb flip takes

    [Header("Collision")]
    [SerializeField] LayerMask obstacleMask = ~0; // layer(s) your cubes are on (default: everything)
    [SerializeField] LayerMask KillMask; // to detect water and lava

    [Header("Gravity")]
    [SerializeField] float gravity = 25f; // how fast the fall accelerates
    [SerializeField] float maxFallSpeed = 20f; // the max speed reached after fall acceleration
    [SerializeField] float killY = -50f; // kills the player if they accidentally noclip out of the world

    bool isMoving; //when true, locks inputs to prevent double input presses etc

    [Header("Dash")]
    [SerializeField] int dashBlocks = 2; // blocks travelled: 2 blocks crossed + the landing cell
    [SerializeField] float dashSpeed = 12f; // blocks per second during the dash
    [SerializeField] float dashCooldown = 3f;
    [SerializeField] Slider DashCooldownSlider;

    [Header("Exit")]
    [SerializeField] LayerMask exitMask; //layermask for the exit stage block]
    [SerializeField] LayerMask paintingMask;

    [Header("GoldCollectible")]
    [SerializeField] LayerMask goldCollectibleMask;

    [Header("Push / Pull")]
    [SerializeField] LayerMask movableMask; // layermask detection for objects with the "movable" tag
    [SerializeField] float pushSpeed = 2f; // speed of which we move the blocks

    float sinkDepth = 1f;
    float sinkDuration = 0.6f;

    float nextDashTime;

    [Header("Block Sounds")]
    [SerializeField] MovableBlockSound[] movableBlockSounds; // array so that it doesnt get messy when adding sfx interactions with future blocks like the plant or more
    [SerializeField] PlayerSound[] playerSounds; //sfx related to the player, so that its tidier instead of using blockSounds for everythingand i have to add redundant sfx to the array
    [field: SerializeField] private EventReference GoldCollected;

    [Header("Player movement SFX")]
    [SerializeField] private EventReference PlayerDash;
    [SerializeField] private EventReference DashError;


    [Header("Cinemachine")]
    [SerializeField] CinemachineVirtualCamera startCam;   // CM2: CinemachineVirtualCamera
    [SerializeField] CinemachineVirtualCamera doorCam;
    [SerializeField] int livePriority = 20;
    [SerializeField] int idlePriority = 0;
    [SerializeField] LayerMask DoorwayMask;
    [SerializeField] bool canToggleCam = true;

    [Header("NPC")]
    [SerializeField] LayerMask NPCMask;
    [SerializeField] Animator npcAnimator;
    [SerializeField] Vector3 npcDetectHalfExtents = new Vector3(1.45f, 0.45f, 1.45f); // 3x3 blocks around the player, same height
    [SerializeField] string npcTrigger = "RunToPainting";

    bool nearNPC;
    bool inDoorZone;

    [SerializeField] GameObject levelCompletePanel;

    
    Transform respawnPoint;
    float respawnDelay = 0.3f; // gonna make it so that the level doesnt restart if player dies, i feel like its very frustrating 
    Vector3 spawnPos;
    Quaternion spawnRot;


    void Awake()
    {
        DashCooldownSlider.gameObject.SetActive(false); // hidden until the first dash
        LevelStarData.ResetAttempt(); //refresh the level data (collectible) when level is reloaded

        spawnPos = respawnPoint != null ? respawnPoint.position : transform.position;
        spawnRot = respawnPoint != null ? respawnPoint.rotation : transform.rotation;
    }

    void Update()
    {
        if (MenuOverlay.IsPaused) return;
        if (levelCompletePanel.activeSelf) return; //to prevent movement during the level complete

        CheckGoldCollect();
        CheckDoorway(transform.position);
        CheckNPC();
        if (canToggleCam && Input.GetKeyDown(KeyCode.T)) (doorCam.Priority, startCam.Priority) = (startCam.Priority, doorCam.Priority); // if player presses t, swap the priority for the cinemachine cameras

        if (isMoving) return; // ignore inputs mid-move
        

        Collider[] exits = Physics.OverlapBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, exitMask, QueryTriggerInteraction.Collide); //detect for exit stage block   
        if (exits.Length > 0)
        {
            ExitStage exit = exits[0].GetComponentInParent<ExitStage>();
            if (exit != null) // save the level progress after level is complete
            {
                exit.Load();
                return;
            }
        }

        Collider[] paintings = Physics.OverlapBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, paintingMask, QueryTriggerInteraction.Collide); //detect for exit stage block   
        if (paintings.Length > 0)
        {
            PaintingsLevelLoader painting = paintings[0].GetComponentInParent<PaintingsLevelLoader>();
            if (painting != null) // save the level progress after level is complete
            {
                painting.Load();
                return;
            }
        }


        Vector3 below = transform.position + Vector3.down;

        if (IsDeadly(transform.position) || IsDeadly(below))
        {
            StartCoroutine(SinkAndDie());
            return;
        }

        if (!IsSolid(below))
        {
            StartCoroutine(Fall());
            return;
        }

        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) dir = new Vector3(-1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.S)) dir = new Vector3(1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.D)) dir = new Vector3(0, 0, 1);
        else if (Input.GetKeyDown(KeyCode.A)) dir = new Vector3(0, 0, -1); //all by 1 as the unity world is built with cubes of 1x1

        if (dir != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.P)) TryPushPull(dir); //initially this was F key, but after playtesting, i got good feedback which said it makes more sense to let the player either use both hands, or dont occupy the fingers positioned at WASD.
            else if (Input.GetKey(KeyCode.Space)) TryDash(dir);
            else TryMove(dir);
        }
    }

    void TryMove(Vector3 dir)
    {
        Vector3 ahead = transform.position + dir;

        if (!IsSolid(ahead))
        {
            StartCoroutine(Slide(ahead, speed)); // nothing in the way: normal move
            PlayWalkSound(ahead + Vector3.down); // detect below us as we're walkin
        }
        else if (!IsSolid(ahead + Vector3.up) && !IsBlockSinking(ahead))
        {
            StartCoroutine(Climb(dir)); // exactly 1 block tall, and there's room to flip over it and prevents climbing if the height infront is 2 blocks instead
            PlayWalkSound(ahead); //detect infront of us as we're climbing
        }
    }
    void Respawn()
    {
        transform.SetPositionAndRotation(spawnPos, spawnRot);
        isMoving = false; // unlock inputs again
    }

    IEnumerator Slide(Vector3 target, float moveSpeed)
    {
        isMoving = true;
        while (transform.position != target) // provokes the gameobjet (the player) to move towards the new world position after inputting movement
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime); //smoothen out the movement via time delta time so it takes 1 second to fully reach the new +!, -1 x/z axis
            yield return null;
        }
        isMoving = false;
    }

    IEnumerator Climb(Vector3 dir)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        Vector3 pivot = startPos + dir * 0.5f + Vector3.up * 0.5f; // pivot = top edge of the block in front (which is also the player's own top-front edge). which allows for a smooth perfect looking rotation

        Vector3 axis = Vector3.Cross(Vector3.up, dir); // Horizontal axis perpendicular to the climb direction which is what allows us to flip over the edge    

        float degreesPerSecond = 180f / climbTime; // turn 180 deg
        float turned = 0f;
        while (turned < 180f)
        {
            float step = Mathf.Min(degreesPerSecond * Time.deltaTime, 180f - turned); //mathf.min prevents it from turning more than 180 deg
            transform.RotateAround(pivot, axis, step); // world-space rotation, doesn't affect input
            turned += step;
            yield return null;
        }


        transform.SetPositionAndRotation(startPos + dir + Vector3.up, Quaternion.AngleAxis(180f, axis) * startRot); // snap to the exact end state so float error never builds up over many climbs.

        isMoving = false;
    }

    bool IsDeadly(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, KillMask, QueryTriggerInteraction.Collide); //same as solid, but makes it so that we dont get blocked by killmasks, and trigger colliders arent ignored.
    bool IsSolid(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, obstacleMask & ~KillMask.value, QueryTriggerInteraction.Ignore); // checks for blocks, what layer and ignore rotation and istrigger colliders,


    IEnumerator Fall()
    {
        isMoving = true;
        float velocity = 0f; //start at zero speed to imitate gravity


        while (!IsSolid(transform.position + Vector3.down) && !IsDeadly(transform.position + Vector3.down)) // keep dropping the player one block at a time until there's a block underneath.
        {
            Vector3 target = transform.position + Vector3.down;

            while (transform.position != target)
            {
                velocity = Mathf.Min(velocity + gravity * Time.deltaTime, maxFallSpeed); //speed = constant acceleration x time, and then account for the max speed we can fall
                transform.position = Vector3.MoveTowards(transform.position, target, velocity * Time.deltaTime);
                yield return null;
            }

            if (transform.position.y < killY)
            {
                Respawn();
                yield break;
            }
        }

        isMoving = false;

        if (IsSolid(transform.position + Vector3.down))
        {
            PlayWalkSound(transform.position + Vector3.down); //sfx when player lands on a block after falling from another place
        }
    }

    void TryDash(Vector3 dir)
    {
        if (Time.time < nextDashTime) return; // if still on cooldown, return
        Vector3 start = transform.position;

        for (int i = 1; i <= dashBlocks; i++) // not really necessary but works if we change the dash distance in the future. how it works is basically by loop, check for any solid blocks in the path of the dash. if so, cancel dash and return
        {
            if (IsSolid(start + dir * i))
            {
                AudioManager.instance.PlayOneShot(DashError, transform.position);
                return;
            }
        }

        nextDashTime = Time.time + dashCooldown; // only start the cooldown on a successful dash
        StartCoroutine(DashCooldownSliderRoutine());
        StartCoroutine(Slide(start + dir * dashBlocks, dashSpeed));
        AudioManager.instance.PlayOneShot(PlayerDash, this.transform.position);
    }
    IEnumerator DashCooldownSliderRoutine()
    {
        DashCooldownSlider.gameObject.SetActive(true);
        DashCooldownSlider.value = 0f;

        while (Time.time < nextDashTime)
        {
            float remaining = nextDashTime - Time.time;
            DashCooldownSlider.value = 1f - (remaining / dashCooldown); // fills up as the cooldown runs out
            yield return null;
        }

        DashCooldownSlider.value = 1f;
        DashCooldownSlider.gameObject.SetActive(false);
    }

    void TryPushPull(Vector3 dir)
    {
        Vector3 pos = transform.position;

        if (GetMovableAt(pos + dir) != null) TryPush(dir); // initially was F and G key for push pull, but i realised i can just make it so that if player holds F, whatever direction they move in will move the block too
        else if (GetMovableAt(pos - dir) != null) TryPull(dir);
    }

    void TryPush(Vector3 dir)
    {
        Vector3 pos = transform.position;

        MovableBlocks block = GetMovableAt(pos + dir); // if the player holds F, but dont press the input key that follows the direction of where ur meant to push the block, return.
        if (block == null || block.IsBusy) return; // method doesnt get called also if theres no block detected, or its busy and already processing an input.

        if (IsSolid(pos + dir * 2)) return; //checks for 2 blocks ahead of player, as long as theres empty space there, push block

        StartCoroutine(MoveWithBlock(pos + dir, block, pos + dir * 2));

        AudioManager.instance.PlayOneShot(GetMovableBlockSound(block.tag).push, transform.position);
    }
    void TryPull(Vector3 dir)
    {
        Vector3 pos = transform.position;

        MovableBlocks block = GetMovableAt(pos - dir); // reverse of push
        if (block == null || block.IsBusy) return;

        if (IsSolid(pos + dir)) return;

        StartCoroutine(MoveWithBlock(pos + dir, block, pos));

        AudioManager.instance.PlayOneShot(GetMovableBlockSound(block.tag).pull, transform.position); //initially used a if,else but realised an array would be more optimised and tidier as ill be adding more blocks like plant ltr on
    }

    IEnumerator MoveWithBlock(Vector3 playerTarget, MovableBlocks block, Vector3 blockTarget)
    {
        isMoving = true;
        block.IsBusy = true; //prevent any other inputs to be stacked

        while (transform.position != playerTarget)
        {
            float step = pushSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTarget, step);
            block.transform.position = Vector3.MoveTowards(block.transform.position, blockTarget, step);
            yield return null;
        }

        block.transform.position = blockTarget;
        block.IsBusy = false;
        isMoving = false;
    }

    MovableBlocks GetMovableAt(Vector3 cellCenter)
    {
        Collider[] hits = Physics.OverlapBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, movableMask, QueryTriggerInteraction.Ignore); //search for movable blocks
        return hits.Length > 0 ? hits[0].GetComponentInParent<MovableBlocks>() : null;
    }
    IEnumerator SinkAndDie()
    {
        isMoving = true;
        PlaySinkSound();

        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * sinkDepth; //initalizing lerp
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / sinkDuration;
            transform.position = Vector3.Lerp(start, end, t); // linear interpolation to make a smooth sinking "animation" when player touches lava or water
            yield return null;
        }
        yield return new WaitForSeconds(respawnDelay);
        Respawn();
    }

    bool IsBlockSinking(Vector3 cell)
    {
        Collider[] hits = Physics.OverlapBox(cell, Vector3.one * 0.45f, Quaternion.identity,
                                             obstacleMask, QueryTriggerInteraction.Ignore);
        foreach (Collider c in hits)
        {
            MovableBlocks block = c.GetComponentInParent<MovableBlocks>();
            if (block != null && block.IsSinking) return true;
        }
        return false;
    }

    MovableBlockSound GetMovableBlockSound(string surfaceTag)
    {
        foreach (MovableBlockSound s in movableBlockSounds)
            if (s.tag == surfaceTag) return s;
        return null;
    }

    PlayerSound GetPlayerSound(string surfaceTag)
    {
        foreach (PlayerSound s in playerSounds)
            if (s.tag == surfaceTag) return s;
        return null;
    }

    string GetSurfaceTag(Vector3 cell) //for detecting floor tags so that diff sfx can be applied for different blocks
    {
        Collider[] hits = Physics.OverlapBox(cell, Vector3.one * 0.45f, Quaternion.identity, obstacleMask & ~KillMask.value, QueryTriggerInteraction.Ignore); // places an invisible box which detects and returns all colliders in it. this allows us to detect the specific tags of diff blocks
        if (hits.Length > 0)
        {
            return hits[0].tag; //if a collider/block is detected, return us the tag and the first one that it has
        }
        else
        {
            return "Untagged"; //if theres nth, then its untagged
        }
    }
    string GetDeadlyTag()
    {
        Collider[] hits = Physics.OverlapBox(transform.position, Vector3.one * 0.45f, Quaternion.identity, KillMask, QueryTriggerInteraction.Collide); //runs the check infront
        if (hits.Length == 0)
        {
            hits = Physics.OverlapBox(transform.position + Vector3.down, Vector3.one * 0.45f, Quaternion.identity, KillMask, QueryTriggerInteraction.Collide); //runs the check below
        }

        if (hits.Length > 0)
        {
            return hits[0].tag; //same as the getsurfacetag but the KillMask detection is added.
        }

        return "Untagged";
    }
    void PlayWalkSound(Vector3 cell) //initially i used audiomanager.instance.playoneshot( such such) on each courotine, but it caused certain bugs so grouping it up to make it tidier and easier to narrow down the fault
    {
        if (AudioManager.instance == null) return;

        string surfaceTag = GetSurfaceTag(cell);
        PlayerSound sound = GetPlayerSound(surfaceTag);
        if (sound == null)
        {
            Debug.LogWarning($"No PlayerSound entry for tag '{surfaceTag}'", this); // tells you which tag is missing
            return;
        }

        AudioManager.instance.PlayOneShot(sound.walk, transform.position);
    }
    void PlaySinkSound()
    {
        if (AudioManager.instance == null) return;

        string deadlyTag = GetDeadlyTag();
        PlayerSound sound = GetPlayerSound(deadlyTag);
        if (sound == null)
        {
            Debug.LogWarning($"No PlayerSound entry for tag '{deadlyTag}'", this); //useful for detecting any bugs in the map in term of sfx, will removei n the future.
            return;
        }

        AudioManager.instance.PlayOneShot(sound.sink, transform.position);
    }

    void CheckGoldCollect()
    {
        Collider[] golds = Physics.OverlapBox(transform.position, Vector3.one * 0.3f, Quaternion.identity, goldCollectibleMask, QueryTriggerInteraction.Collide); //check for gold collectible 
        foreach (Collider gold in golds)
        {
            LevelStarData.CollectGold();

            if (AudioManager.instance != null) AudioManager.instance.PlayOneShot(GoldCollected, gold.transform.position);

            Destroy(gold.gameObject);
        }
    }

    bool IsDoorway(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, DoorwayMask, QueryTriggerInteraction.Collide);  //check for the doorway zone
    void CheckDoorway(Vector3 cellCenter) // change pov when we enter the camera zone
    {
        bool inside = IsDoorway(cellCenter);
        if (inside == inDoorZone) return; // nothing changed since last frame

        inDoorZone = inside;

        doorCam.Priority = inside ? livePriority : idlePriority; // priorities only change if we are actually in the layermask
        startCam.Priority = inside ? idlePriority : livePriority;
    }
    void CheckNPC()
    {
        bool near = Physics.CheckBox(transform.position, npcDetectHalfExtents, Quaternion.identity, NPCMask, QueryTriggerInteraction.Collide);

        if (near == nearNPC) return; //if player isnt near = within the range of the npc, nothing happens
        nearNPC = near;

        if (near) npcAnimator.SetTrigger(npcTrigger); // if so, then trigger the animation
    }
}