using System.Collections;
using UnityEngine;

public class RotatingPlatform : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] Vector3 worldAxis = Vector3.up; // Y axis, of which the platform swings around
    [SerializeField] float angle = 90f; // degrees per activation via lever or a pressure plate
    [SerializeField] float duration = 0.5f; // seconds per rotation

    public bool IsRotating { get; private set; }

    Quaternion startRotation; // remembers the initial rotation values of the object at the start
    int timesRotated; 

    void Awake()
    {
        startRotation = transform.rotation;
    }

    // Call this from a lever, pressure plate, etc.
    public void Rotate()
    {
        if (IsRotating) return; // ignore multiple inputs mid-rotation
        StartCoroutine(RotateRoutine());
    }

    IEnumerator RotateRoutine()
    {
        IsRotating = true; 

        float fromAngle = angle * timesRotated; // where the current platform's angle is at, after set rotations
        timesRotated++; // +1 rotation
        float toAngle = angle * timesRotated; // new angle to move the platform to, compared to current platform's angle, after set rotations

        float time = 0f;
        float length = Mathf.Max(duration, 0.01f); //how we'll calculate the duration of which the platform will rotate for

        while (time < length)
        {
            time += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, time / length); // interpolates between the start and end or to and fro
            float current = Mathf.Lerp(fromAngle, toAngle, t); 
            transform.rotation = Quaternion.AngleAxis(current, worldAxis) * startRotation; //set the new rotational value
            yield return null;
        }

        transform.rotation = Quaternion.AngleAxis(toAngle, worldAxis) * startRotation; //snaps to the world grid just in case it goes out of space
        IsRotating = false;
    }
}