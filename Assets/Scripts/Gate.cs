using UnityEngine;

public class Gate : Activatable
{
    [Header("Gate")]
    [SerializeField] GameObject gateObject; // leave empty to use the object this script is on

    GameObject Target => gateObject != null ? gateObject : gameObject;

    public override void Activate() => Target.SetActive(false); // open
    public override void Deactivate() => Target.SetActive(true);  // close
}