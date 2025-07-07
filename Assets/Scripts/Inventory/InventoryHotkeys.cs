using UnityEngine;

public class InventoryHotkeys : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UseSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseSlot(2);
    }

    private void UseSlot(int idx)
    {
        var slots = InventoryManager.Instance.itemSlots;
        if (idx < 0 || idx >= slots.Count) return;

        ItemSlot s = slots[idx];
        if (s != null && s.m_item != null)
            s.OnUseItem();
    }
}

