using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothness = 5f;

    Vector3 offset;

    void Start()
    {
        if (target == null) return;

        offset = transform.position - target.position; // the change in position when the player moves
    }

    void LateUpdate() // runs after the player's coroutines have moved, which avoids jitter
    {
        if (target == null) return;

        Vector3 desired = target.position + offset;

        float t = 1f - Mathf.Exp(-smoothness * Time.deltaTime); //frame-rate independent lerp initalization
        transform.position = Vector3.Lerp(transform.position, desired, t);
    }
}