using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public PickupType pickupType; 
    public Sprite icon = null;
    public bool isStackable = false;
    public int maxStack = 1;
    public string description = "Item description here";

    // You can add more properties here, like:
    // public int damage;
    // public float weight;
    // public ItemType itemType;
}
