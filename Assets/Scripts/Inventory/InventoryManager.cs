using UnityEngine;
using System.Collections.Generic;


public class InventoryManager : MonoBehaviour
{
    private List<PickupType> inventory = new();

    private void OnEnable()
    {
        EventBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        inventory.Add(e.pickupType);
        Debug.Log($"[Inventory] Added: {e.pickupType} | Total items: {inventory.Count}");
    }

}
