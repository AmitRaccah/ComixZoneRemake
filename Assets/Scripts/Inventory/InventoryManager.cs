using System;
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List <Item> allItems = new List<Item>();
    public List<Item> inventory = new List<Item>();
    public List <ItemSlot> itemSlots;

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
            if (itemSlots[i].m_item == null)
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
        Item newItem = null;
        newItem = allItems.Find(x => x.pickupType == e.pickupType);
        inventory.Add(newItem);
        Debug.Log($"[Inventory] Added: {e.pickupType} | Total items: {inventory.Count}");

        ManageItemSlots(newItem);
    }

    private void ManageItemSlots(Item _newItem)
    {
        ItemSlot slot = itemSlots.Find(x => x.m_item == null);
        slot.Initialize(_newItem);
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
