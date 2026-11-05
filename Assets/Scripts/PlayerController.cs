using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed = 2.5f;      // squares per second when sliding
    [SerializeField] float climbTime = 0.5f;  // seconds the climb flip takes

    [Header("Collision")]
    [SerializeField] LayerMask obstacleMask = ~0; // layer(s) your cubes are on (default: everything)

    bool isMoving;

    void Update()
    {
        if (isMoving) return; // ignore input mid-move

        // Always WORLD-space directions, so WASD means the same thing no matter how the player is rotated.
        // (Never use transform.forward / right / up for input.)
        Vector3 dir = Vector3.zero;
        if (Input.GetKeyDown(KeyCode.W)) dir = new Vector3(-1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.S)) dir = new Vector3(1, 0, 0);
        else if (Input.GetKeyDown(KeyCode.D)) dir = new Vector3(0, 0, 1);
        else if (Input.GetKeyDown(KeyCode.A)) dir = new Vector3(0, 0, -1);

        if (dir != Vector3.zero) TryMove(dir);
    }

    void TryMove(Vector3 dir)
    {
        Vector3 pos = transform.position;
        Vector3 front = pos + dir;

        if (!IsSolid(front))
        {
            StartCoroutine(Slide(front)); // nothing in the way: normal move
        }
        else if (!IsSolid(front + Vector3.up) && !IsSolid(pos + Vector3.up))
        {
            StartCoroutine(Climb(dir));   // exactly 1 cube tall, and there's room to flip over it
        }
        // otherwise: 2+ cubes tall (or a ceiling above the player) -> blocked, do nothing
    }

    IEnumerator Slide(Vector3 target)
    {
        isMoving = true;
        while (transform.position != target)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }
        isMoving = false;
    }

    IEnumerator Climb(Vector3 dir)
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        // Pivot = top edge of the cube in front (which is also the player's own top-front edge).
        Vector3 pivot = startPos + dir * 0.5f + Vector3.up * 0.5f;

        // Horizontal axis perpendicular to the climb direction -> he flips up and over the edge.
        Vector3 axis = Vector3.Cross(Vector3.up, dir);

        float degreesPerSecond = 180f / climbTime;
        float turned = 0f;
        while (turned < 180f)
        {
            float step = Mathf.Min(degreesPerSecond * Time.deltaTime, 180f - turned);
            transform.RotateAround(pivot, axis, step); // world-space rotation, doesn't affect input
            turned += step;
            yield return null;
        }

        // Snap to the exact end state so float error never builds up over many climbs.
        transform.SetPositionAndRotation(
            startPos + dir + Vector3.up,
            Quaternion.AngleAxis(180f, axis) * startRot);

        isMoving = false;
    }

    // Is there a cube occupying the grid cell centred at this point?
    bool IsSolid(Vector3 cellCenter) =>
        Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity,
                         obstacleMask, QueryTriggerInteraction.Ignore);
}