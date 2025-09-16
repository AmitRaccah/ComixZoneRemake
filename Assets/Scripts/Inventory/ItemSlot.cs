using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item m_item = null;

    [SerializeField] private Image image;
    [SerializeField] private Sprite frameSprite;

    private void Reset()
    {
        image = GetComponent<Image>();
        if (image != null && frameSprite == null)
            frameSprite = image.sprite;
    }

    private void Awake()
    {
        if (image == null) image = GetComponent<Image>();
        if (frameSprite == null && image != null)
            frameSprite = image.sprite;

        ApplyVisual(m_item);
    }

    private void OnValidate()
    {
        if (image == null) image = GetComponent<Image>();
        if (frameSprite == null && image != null)
            frameSprite = image.sprite;
    }

    private void ApplyVisual(Item item)
    {
        if (image == null) return;

        image.sprite = frameSprite;

        if (item != null && item.icon != null)
            image.overrideSprite = item.icon;
        else
            image.overrideSprite = null; 

        image.enabled = true;
    }

    public void Initialize(Item item)
    {
        m_item = item;
        ApplyVisual(item);
    }

    public void Clear()
    {
        m_item = null;
        ApplyVisual(null); 
    }

    public void OnUseItem()
    {
        InventoryManager.Instance.TryUseSlot(this);
    }
}
