using System;
using UnityEngine;
using System.Collections.Generic;


public class InventoryManager : MonoBehaviour
{
    private List<PickupType> inventory = new();
    private List<InvItem> inventoryItems = new List<InvItem>();

    public List <ItemSlot> itemSlots;
    private void OnEnable()
    {
        CoreBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        inventory.Add(e.pickupType);
        inventoryItems.Add(new InvItem(e.pickupType, null ));
        Debug.Log($"[Inventory] Added: {e.pickupType} | Total items: {inventory.Count}");

        ManageItemSlots();
    }

    private void ManageItemSlots()
    {
        itemSlots[0].Initialize(inventoryItems[0]);
    }
}

[System.Serializable]
public struct InvItem
{
    public PickupType p_type;
    public Sprite itmeSPrite;

    public InvItem(PickupType p_type, Sprite itmeSprite)
    {
        this.p_type = p_type;
        this.itmeSPrite = itmeSprite;
    }
}
