using UnityEngine;
using System.Collections;

public class BlinkingArrow : MonoBehaviour
{
    [SerializeField] private float onDuration = 0.5f;
    [SerializeField] private float offDuration = 0.5f;
    [SerializeField] private int blinkCycles = 6;

    private Renderer[] renderersInChildren;
    private Coroutine blinkCo;

    void Awake()
    {
        renderersInChildren = GetComponentsInChildren<Renderer>(true);
    }

    void OnEnable()
    {
        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(BlinkRoutine());
    }

    void OnDisable()
    {
        if (blinkCo != null)
        {
            StopCoroutine(blinkCo);
            blinkCo = null;
        }
    }

    IEnumerator BlinkRoutine()
    {
        for (int i = 0; i < blinkCycles; i++)
        {
            SetVisible(true);
            yield return new WaitForSeconds(onDuration);
            SetVisible(false);
            yield return new WaitForSeconds(offDuration);
        }
        SetVisible(true);
        blinkCo = null;
    }

    void SetVisible(bool v)
    {
        if (renderersInChildren == null) return;
        for (int i = 0; i < renderersInChildren.Length; i++)
        {
            if (renderersInChildren[i] != null)
                renderersInChildren[i].enabled = v;
        }
    }
}
