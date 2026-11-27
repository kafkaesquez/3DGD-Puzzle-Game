using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents instance {  get; private set; }

    [field: Header("Player SFX")]
    [field: SerializeField] public EventReference playerMovement { get; private set; } // field: puts serializefield on the hidden field behind the property, so it shows in inspector but stays read only to other scripts


    [field: Header("Music")]
    [field: SerializeField] public EventReference Level1Track { get; private set; } 

    private void Awake()
    {
        if (instance == null) instance = this;
    }
}

[System.Serializable]
public class MovableBlockSound //note fore self reference - might wanna add randomizer to vary the sfx when pushing to not make the soundscape super repetitive
{
    public string tag; // the block's tag
    public EventReference push;
    public EventReference pull;
}

[System.Serializable]
public class PlayerSound //note fore self reference - might wanna add randomizer to vary the sfx when pushing to not make the soundscape super repetitive
{
    public string tag; // the block's tag
    public EventReference walk;
    public EventReference sink;
}

[System.Serializable]
public class SinkingSound //note fore self reference - might wanna add randomizer to vary the sfx when pushing to not make the soundscape super repetitive
{
    public string tag; // the block's tag
    public EventReference sink;
}