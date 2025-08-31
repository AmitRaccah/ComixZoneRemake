using UnityEngine;

#region Core / Inventory 🗂️
public struct PlayerPickUpEvent { }                
public struct ItemPickedUpEvent { public PickupType pickupType; }
public struct ItemUsedEvent { public PickupType pickupType; }
public struct InventoryChangedEvent { }
#endregion
public struct PotionConsumedEvent
{
    public int healAmount;
    public PotionConsumedEvent(int amount)
    {
        healAmount = amount;
    }
}