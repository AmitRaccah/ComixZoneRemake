using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
   //ITEM reference
   public InvItem m_item;
   [SerializeField] private Image itemImage;
   [SerializeField] private TextMeshProUGUI itemAmount_txt;

   public void Initialize(InvItem item)
   {
      m_item = item;
      //itemImage.sprite = pickupItem.itmeSPrite;
   }
}
