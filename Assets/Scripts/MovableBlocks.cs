using System.Collections;
using UnityEngine;

public class MovableBlocks : MonoBehaviour
{
    [SerializeField] LayerMask obstacleMask = 3; //ground layer

    [Header("Gravity")]
    [SerializeField] float gravity = 25f; // same stuff as player controller
    [SerializeField] float maxFallSpeed = 20f;      

    public bool IsBusy { get; set; } //true while the block is falling or being pushed/pulled to prevent double inputs

    void Update()
    {
        if (IsBusy) return;

        if (!IsSolid(transform.position + Vector3.down)) //check for blocks below it. if there isnt, then fall downwards
        {
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        IsBusy = true;
        float velocity = 0f;

        while (!IsSolid(transform.position + Vector3.down))
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

    bool IsSolid(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore); //same in playercontroller, and ignores istrigger type of colliders.
}