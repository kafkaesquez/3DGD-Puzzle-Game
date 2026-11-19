using FMODUnity;
using UnityEngine;

public class LeverInteraction : MonoBehaviour
{
    [Header("LayerMask")]
    [SerializeField] LayerMask LeverBlock;

    [Header("Platform")]
    [SerializeField] RotatingPlatform Platform; // drag the platform that gets rotated here

    [Header("AudioClip")]
    [SerializeField] EventReference LeverClick;

    static readonly Vector3[] Directions =  // directions array, as player doesnt have a set direction that he faces, as long as we are beside the block, this helps us to detect it
    {
        Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up,
    };

    bool IsLever(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, LeverBlock, QueryTriggerInteraction.Collide); // same logic as the other bool Is____

    // true if any of the 4 neighbouring cells holds a lever block
    bool LeverNextToPlayer()
    {
        foreach (Vector3 dir in Directions)
        {
            if (IsLever(transform.position + dir)) return true;
        }
        return false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && LeverNextToPlayer())
        {
            LeverInteracted();
        }
    }

    void LeverInteracted()
    {
        if (Platform == null || Platform.IsRotating) return;

        Platform.Rotate();
        AudioManager.instance.PlayOneShot(LeverClick, transform.position);
    }
}