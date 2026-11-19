using UnityEngine;

public abstract class Activatable : MonoBehaviour //base class for the pressure plate
{
    public abstract void Activate(); // plate pressed
    public abstract void Deactivate(); // plate released
    public virtual bool IsBusy => false; //to prevent multiple inputs
}