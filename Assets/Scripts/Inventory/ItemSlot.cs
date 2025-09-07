using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item m_item = null;
    [SerializeField] private Image itemImage;

    public void Initialize(Item item)
    {
        m_item = item;
        if (itemImage != null)
            itemImage.sprite = (item != null) ? item.icon : null;
    }

    public void OnUseItem()
    {
        InventoryManager.Instance.TryUseSlot(this);
    }

    public void Clear()
    {
        m_item = null;
        if (itemImage != null) itemImage.sprite = null;
    }
}
