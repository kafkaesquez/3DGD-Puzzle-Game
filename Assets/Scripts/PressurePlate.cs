using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] LayerMask activatorMask; //movable blocks and the player will set the pressure plate on

    [Header("Targets")]
    [SerializeField] Activatable[] targets; // array list of what can be activated and deactivated. makes it easier for future level building as i can reference platforms and gates + i can activate multiple things

    bool appliedState; // the previous state the target is in, activated/deactivated

    bool IsOccupied(Vector3 cellCenter) => Physics.CheckBox(cellCenter, Vector3.one * 0.45f, Quaternion.identity, activatorMask, QueryTriggerInteraction.Collide);

    bool TargetsBusy() //to prevent multiple inputs
    {
        foreach (Activatable target in targets)
        {
            if (target != null && target.IsBusy) return true;
        }
        return false;
    }

    void Update()
    {
        bool pressed = IsOccupied(transform.position);

        if (pressed == appliedState) return; // nothing to change
        if (TargetsBusy()) return;

        appliedState = pressed;

        foreach (Activatable target in targets)
        {
            if (target == null) continue;

            if (pressed) target.Activate();
            else target.Deactivate();
        }
    }
}