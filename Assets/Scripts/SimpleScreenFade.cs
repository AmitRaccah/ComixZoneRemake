using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class SimpleScreenFade : MonoBehaviour
{
    private CanvasGroup cg;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;                
    }

    public Tween Fade(float to, float dur) =>
        cg.DOFade(to, dur).SetEase(Ease.Linear);
}
