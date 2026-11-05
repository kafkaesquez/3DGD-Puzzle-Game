using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 2.5f; // squares per second when sliding
    [SerializeField] float climbTime = 0.5f; // seconds the climb flip takes

    [Header("Collision")]
    [SerializeField] LayerMask obstacleMask = ~0; // layer(s) your cubes are on (default: everything)

    [Header("Gravity")]
    [SerializeField] float gravity = 25f; // how fast the fall accelerates
    [SerializeField] float maxFallSpeed = 20f; // the max speed reached after fall acceleration
    [SerializeField] float killY = -50f; // kills the player if they accidentally noclip out of the world

    bool isMoving; //when true, locks inputs to prevent double input presses etc

    [Header("Dash")]
    [SerializeField] int dashBlocks = 2; // blocks travelled: 2 blocks crossed + the landing cell
    [SerializeField] float dashSpeed = 12f; // blocks per second during the dash
    [SerializeField] float dashCooldown = 3f;

    [Header("Push / Pull")]
    [SerializeField] LayerMask movableMask; // layermask detection for objects with the "movable" tag
    [SerializeField] float pushSpeed = 2f; // speed of which we move the blocks

    float nextDashTime;


    void Update()
    {
        if (isMoving) return; // ignore input mid-move
        if (!IsSolid(transform.position + Vector3.down))
        {
            StartCoroutine(Fall());
            return;
        }

        // Always WORLD-space directions, so WASD means the same thing no matter how the player is rotated.
        // (Never use transform.forward / right / up for input.)
        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) dir = new Vector3(-1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.S)) dir = new Vector3(1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.D)) dir = new Vector3(0, 0, 1);
        else if (Input.GetKeyDown(KeyCode.A)) dir = new Vector3(0, 0, -1);

        if (dir != Vector3.zero)
        {
            if (Input.GetKey(KeyCode.F)) TryPush(dir);
            else if (Input.GetKey(KeyCode.G)) TryPull(dir);
            else if (Input.GetKey(KeyCode.Space)) TryDash(dir);
            else TryMove(dir);
        }
    }

    void TryMove(Vector3 dir)
    {
        Vector3 pos = transform.position;
        Vector3 front = pos + dir;

        if (!IsSolid(front))
        {
            StartCoroutine(Slide(front, speed)); // nothing in the way: normal move
        }
        else if (!IsSolid(front + Vector3.up) && !IsSolid(pos + Vector3.up))
        {
            StartCoroutine(Climb(dir)); // exactly 1 block tall, and there's room to flip over it and prevents climbing if the height infront is 2 blocks instead
        }
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

        Vector3 axis = Vector3.Cross(Vector3.up, dir); // Horizontal axis perpendicular to the climb direction -> he flips up and over the edge.

        float degreesPerSecond = 180f / climbTime; // turn 180 deg
        float turned = 0f;
        while (turned < 180f)
        {
            float step = Mathf.Min(degreesPerSecond * Time.deltaTime, 180f - turned);
            transform.RotateAround(pivot, axis, step); // world-space rotation, doesn't affect input
            turned += step;
            yield return null;
        }

        
        transform.SetPositionAndRotation(startPos + dir + Vector3.up, Quaternion.AngleAxis(180f, axis) * startRot); // snap to the exact end state so float error never builds up over many climbs.

        isMoving = false;
    }


    bool IsSolid(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore); // checks for blocks, what layer and ignore rotation and istrigger colliders,

    IEnumerator Fall()
    {
        isMoving = true;
        float velocity = 0f; //start at zero speed to imitate gravity

        
        while (!IsSolid(transform.position + Vector3.down))// keep dropping the player one block at a time until there's a block underneath.
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
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // if the player clips out and falls out of the world, reload the level (note for self, add UI for lose screen and then restart button.
                yield break; //stop the courotine so nothing else runs while scene is reloaded.
            }
        }

        isMoving = false;
    }

    void TryDash(Vector3 dir)
    {
        if (Time.time < nextDashTime) return; // if still on cooldown, return

        Vector3 start = transform.position;

        for (int i = 1; i <= dashBlocks; i++) // not really necessary but works if we change the dash distance in the future. how it works is basically by loop, check for any solid blocks in the path of the dash. if so, cancel dash and return
        {
            if (IsSolid(start + dir * i)) return;
        }

        nextDashTime = Time.time + dashCooldown; // only start the cooldown on a successful dash
        StartCoroutine(Slide(start + dir * dashBlocks, dashSpeed));
    }

    void TryPush(Vector3 dir)
    {
        Vector3 pos = transform.position;

        MovableBlocks block = GetMovableAt(pos + dir); // if the player holds F, but dont press the input key that follows the direction of where ur meant to push the block, return.
        if (block == null || block.IsBusy) return; // method doesnt get called also if theres no block detected, or its busy and already processing an input.

        if (IsSolid(pos + dir * 2)) return;

        StartCoroutine(MoveWithBlock(pos + dir, block, pos + dir * 2)); 
    }
    void TryPull(Vector3 dir)
    {
        Vector3 pos = transform.position;

        MovableBlocks block = GetMovableAt(pos - dir); // reverse of push
        if (block == null || block.IsBusy) return;

        if (IsSolid(pos + dir)) return;

        StartCoroutine(MoveWithBlock(pos + dir, block, pos));
    }

    IEnumerator MoveWithBlock(Vector3 playerTarget, MovableBlocks block, Vector3 blockTarget)
    {
        isMoving = true;
        block.IsBusy = true;

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
        Collider[] hits = Physics.OverlapBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, movableMask, QueryTriggerInteraction.Ignore);
        return hits.Length > 0 ? hits[0].GetComponentInParent<MovableBlocks>() : null;
    }
}