using UnityEngine;

public struct ItemPickedUpEvent
{
    public PickupType pickupType;

    public ItemPickedUpEvent(PickupType pickupType) 
    {
        this.pickupType = pickupType;
    }
}