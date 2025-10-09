using UnityEngine;

public class TutorialBalloonWorld : MonoBehaviour
{
    [SerializeField] private SpriteRenderer bubbleRenderer;

    public void Show(Sprite s)
    {
        if (!bubbleRenderer) return;
        bubbleRenderer.sprite = s;
        bubbleRenderer.enabled = true;
        var c = bubbleRenderer.color; c.a = 1f; bubbleRenderer.color = c;
    }

    public void Hide()
    {
        if (!bubbleRenderer) return;
        bubbleRenderer.enabled = false;
    }
}
