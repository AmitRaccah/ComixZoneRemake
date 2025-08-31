using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item m_item = null;
    [SerializeField] private Image itemImage;
    public void Initialize(Item item)
    {
        m_item = item;
        itemImage.sprite = item.icon;
    }
    public void OnUseItem()
    {
        if (m_item == null) return;
        m_item.Use();  
        InventoryManager.Instance.inventory.Remove(m_item);
        CoreBus.Publish(new InventoryChangedEvent());
        m_item = null;
        itemImage.sprite = null;
    }
}