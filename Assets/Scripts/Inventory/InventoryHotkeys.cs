//using UnityEngine;

//public class InventoryHotkeys : MonoBehaviour
//{
//    void Update()
//    {
//        if (Input.GetKeyDown("u")) UseSlot(0);
//        if (Input.GetKeyDown("i")) UseSlot(1);
//        if (Input.GetKeyDown("o")) UseSlot(2);
//    }

//    void UseSlot(int idx)
//    {
//        var slots = InventoryManager.Instance.itemSlots;
//        if (idx < 0 || idx >= slots.Count) return;

//        var s = slots[idx];
//        if (s != null && s.m_item != null)
//            s.OnUseItem();
//    }
//}
