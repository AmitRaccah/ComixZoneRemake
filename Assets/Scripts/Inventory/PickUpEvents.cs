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

public struct KnifeThrownEvent
{
    public int damage;
    public float speed;
    public float distance;
    public float rotationSpeed;

    public KnifeThrownEvent(int dmg, float spd, float dist, float rotSpeed)
    {
        damage = dmg;
        speed = spd;
        distance = dist;
        rotationSpeed = rotSpeed;
    }
}