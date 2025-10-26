using UnityEngine;

public abstract class Item : ScriptableObject
{
    public Sprite icon;
    public PickupType pickupType;
    public AudioCue pickupSuccessCue;
    public AudioCue pickupFailCue;

    public virtual bool Use()
    {
        return true;
    }

    public virtual bool IsConsumable
    {
        get { return true; }
    }
}
