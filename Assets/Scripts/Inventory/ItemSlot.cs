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

    public void OnUseItem() // call when active item!
    {
        // Remove this specific item from inventory list
        InventoryManager.Instance.inventory.Remove(m_item);
        
        // Clear the slot
        m_item = null;
        itemImage.sprite = null;
        Debug.Log("Item used and slot cleared!");
        
    }
}
