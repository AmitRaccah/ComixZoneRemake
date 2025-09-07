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
        AddItem(e.pickupType);
    }

    public bool TryAddItem(PickupType type) => AddItem(type);

    public bool TryUseSlot(ItemSlot slot)
    {
        if (slot == null || slot.m_item == null) return false;

        var item = slot.m_item;
        bool used = item.Use();
        if (!used) return false;

        if (item.IsConsumable)
        {
            inventory.Remove(item);
            slot.Clear();
            CoreBus.Publish(new InventoryChangedEvent());
        }
        return true;
    }

    public bool AddItem(PickupType type)
    {
        for (int i = 0; i < inventory.Count; i++)
            if (inventory[i] != null && inventory[i].pickupType == type)
                return false;

        ItemSlot slot = itemSlots.Find(s => s != null && s.m_item == null);
        if (slot == null) { Debug.Log("Inventory Full!"); return false; }

        Item newItem = null;
        for (int i = 0; i < allItems.Count; i++)
            if (allItems[i] != null && allItems[i].pickupType == type)
            { newItem = allItems[i]; break; }

        if (newItem == null) { Debug.LogWarning($"AddItem: Item not found for {type}"); return false; }

        inventory.Add(newItem);
        slot.Initialize(newItem);

        CoreBus.Publish(new InventoryChangedEvent());
        return true;
    }


}