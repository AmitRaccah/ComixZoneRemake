using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TutorialBalloonWorld : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bubbleRenderer;

    void Awake()
    {
        if (!bubbleRenderer) bubbleRenderer = GetComponent<SpriteRenderer>();
        if (bubbleRenderer) bubbleRenderer.enabled = false;
    }

    public void Show(Sprite sprite)
    {
        if (!bubbleRenderer) return;
        bubbleRenderer.sprite = sprite;
        bubbleRenderer.enabled = sprite != null;
    }

    public void Hide()
    {
        if (!bubbleRenderer) return;
        bubbleRenderer.enabled = false;
    }
}
