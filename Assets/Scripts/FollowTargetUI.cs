using UnityEngine;

public class FollowTargetUI : MonoBehaviour
{
    [SerializeField] Transform target; //the player cube
    [SerializeField] Vector3 worldOffset = new Vector3(0, 1.5f, 0); // where the cooldown sits
    [SerializeField] Camera cam; //the camera reference

    RectTransform rect;

    void Awake()
    {
        rect = (RectTransform)transform;
        if (cam == null) cam = Camera.main;
    }

    void LateUpdate()
    {
        if (target == null) return;
        rect.position = cam.WorldToScreenPoint(target.position + worldOffset);
    }
}