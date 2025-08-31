using UnityEngine;

public abstract class Item : ScriptableObject
{
    public PickupType pickupType;
    public Sprite icon = null;
    public bool isStackable = false;
    public int maxStack = 1;
    public string description = "Item description here";

    public virtual void Use()
    {

    }
}