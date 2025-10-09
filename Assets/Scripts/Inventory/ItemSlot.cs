using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item m_item;
    [SerializeField] private Image iconImage;

    void Awake()
    {
        if (!iconImage)
        {
            var t = transform.Find("Icon");
            if (t) iconImage = t.GetComponent<Image>();
        }
        ApplyVisual(m_item);
    }

    void OnValidate()
    {
        if (!iconImage)
        {
            var t = transform.Find("Icon");
            if (t) iconImage = t.GetComponent<Image>();
        }
        ApplyVisual(m_item);
    }

    void ApplyVisual(Item item)
    {
        if (!iconImage) return;
        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
    }

    public void Initialize(Item item) { m_item = item; ApplyVisual(item); }
    public void Clear() { m_item = null; ApplyVisual(null); }
    public void OnUseItem() { InventoryManager.Instance.TryUseSlot(this); }
}
