using System;
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> allItems = new List<Item>();
    public List<Item> inventory = new List<Item>();
    public List<ItemSlot> itemSlots;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
    }

    public bool IsFreeSlot()
    {
        int freeSlots = 0;
        for (int i = 0; i < itemSlots.Count; i++)
        {
            if (itemSlots[i] != null && itemSlots[i].m_item == null)
                freeSlots++;
        }
        if (freeSlots > 0)
        {
            return true;
        }
        Debug.Log("Inventory Full!");
        return false;
    }

    private void OnItemPickedUp(ItemPickedUpEvent e)
    {
        inventory.RemoveAll(it => it == null);
        if (inventory.Exists(it => it.pickupType == e.pickupType)) return;
        if (!IsFreeSlot()) return;
        Item newItem = allItems.Find(x => x.pickupType == e.pickupType);
        if (newItem == null)
        {
            return;
        }
        inventory.Add(newItem);
        ManageItemSlots(newItem);
        CoreBus.Publish(new InventoryChangedEvent());
    }

    public bool TryAddItem(PickupType type)
    {
        for (int i = 0; i < inventory.Count; i++)
            if (inventory[i] != null && inventory[i].pickupType == type)
                return false;
        if (!IsFreeSlot()) return false;
        Item newItem = null;
        for (int i = 0; i < allItems.Count; i++)
            if (allItems[i] != null && allItems[i].pickupType == type)
            { newItem = allItems[i]; break; }
        if (newItem == null) return false;
        ItemSlot slot = itemSlots.Find(s => s.m_item == null);
        if (slot == null)
            return false;
        inventory.Add(newItem);
        slot.Initialize(newItem);
        CoreBus.Publish(new InventoryChangedEvent());
        return true;
    }

    private void ManageItemSlots(Item newItem)
    {
        ItemSlot slot = itemSlots.Find(s => s.m_item == null);
        if (slot == null)
        {
            Debug.LogError("No free slot found");
            return;
        }
        slot.Initialize(newItem);
    }
}