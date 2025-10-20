using UnityEngine;

public struct PlayerPickUpEvent { }
public struct ItemPickedUpEvent { public PickupType pickupType; }
public struct ItemUsedEvent { public PickupType pickupType; }
public struct InventoryChangedEvent { }

public struct PotionConsumedEvent
{
    public int healAmount;
    public PotionConsumedEvent(int amount) { healAmount = amount; }
}

public struct KnifeThrownEvent
{
    public string knifeId;
    public AttackData attackData;
    public float speed;
    public float distance;
    public Vector3 spinPerSecond;

    public KnifeThrownEvent(string id, AttackData data, float spd, float dist, Vector3 spin)
    {
        knifeId = id;
        attackData = data;
        speed = spd;
        distance = dist;
        spinPerSecond = spin;
    }
}
