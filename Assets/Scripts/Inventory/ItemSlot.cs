using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    //ITEM reference
    public Item m_item = null;
    [SerializeField] private Image itemImage;
    //[SerializeField] private TextMeshProUGUI itemAmount_txt;

    public void Initialize(Item item)
    {
        m_item = item;
        itemImage.sprite = item.icon;
    }

    public void OnUseItem()
    {
        InventoryManager.Instance.inventory.Remove(m_item);

        CoreBus.Publish(new ItemUsedEvent { pickupType = m_item.pickupType });
        CoreBus.Publish(new InventoryChangedEvent());

        m_item = null;
        itemImage.sprite = null;
    }
}