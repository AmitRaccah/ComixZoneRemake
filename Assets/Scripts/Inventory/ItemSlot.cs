using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public Item m_item = null;

    // Assign in Inspector: this is the SAME Image that shows the yellow frame.
    [SerializeField] private Image image;
    // Safety: assign the frame sprite (your yellow "NewSlot") here.
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

        // Draw initial state (no item -> frame only)
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

        // Always keep the frame as the base sprite
        image.sprite = frameSprite;

        // If we have an item icon, draw it as an override on top of the frame
        if (item != null && item.icon != null)
            image.overrideSprite = item.icon;
        else
            image.overrideSprite = null; // back to "just the frame"

        // Never hide the frame
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
        ApplyVisual(null); // show only the frame
    }

    public void OnUseItem()
    {
        InventoryManager.Instance.TryUseSlot(this);
    }
}
