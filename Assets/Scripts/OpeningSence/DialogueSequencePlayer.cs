using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class DialogueStep
{
    public GameObject bubble;
    public GameObject textObj;
    public float duration = 2f;
}

public class DialogueSequencePlayer : MonoBehaviour
{
    public DialogueStep[] steps;

    public float scaleInTime = 0.15f;
    public float scaleOutTime = 0.15f;

    Dictionary<Transform, Vector3> originalScale = new Dictionary<Transform, Vector3>();

    void Awake()
    {
        CacheOriginalScales();
    }

    void CacheOriginalScales()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            var s = steps[i];
            if (s == null) continue;

            if (s.bubble != null)
            {
                var t = s.bubble.transform;
                if (!originalScale.ContainsKey(t))
                    originalScale.Add(t, t.localScale);
            }

            if (s.textObj != null)
            {
                var t = s.textObj.transform;
                if (!originalScale.ContainsKey(t))
                    originalScale.Add(t, t.localScale);
            }
        }
    }

    Vector3 GetOriginalScale(Transform t)
    {
        if (t == null) return Vector3.one;
        if (originalScale.TryGetValue(t, out var s))
            return s;
        return t.localScale;
    }

    public IEnumerator PlaySequence()
    {
        HideAll();

        for (int i = 0; i < steps.Length; i++)
        {
            DialogueStep s = steps[i];
            if (s == null) continue;

            HideAll();

            yield return StartCoroutine(PlayStep(s));
        }

        HideAll();
    }

    IEnumerator PlayStep(DialogueStep s)
    {
        Transform bubbleT = s.bubble ? s.bubble.transform : null;
        Transform textT = s.textObj ? s.textObj.transform : null;

        if (s.bubble) s.bubble.SetActive(true);
        if (s.textObj) s.textObj.SetActive(true);

        if (bubbleT) bubbleT.localScale = Vector3.zero;
        if (textT) textT.localScale = Vector3.zero;

        Vector3 bubbleTarget = GetOriginalScale(bubbleT);
        Vector3 textTarget = GetOriginalScale(textT);

        float tIn = 0f;
        while (tIn < scaleInTime)
        {
            tIn += Time.deltaTime;
            float a = (scaleInTime <= 0f) ? 1f : Mathf.Clamp01(tIn / scaleInTime);

            if (bubbleT) bubbleT.localScale = Vector3.LerpUnclamped(Vector3.zero, bubbleTarget, a);
            if (textT) textT.localScale = Vector3.LerpUnclamped(Vector3.zero, textTarget, a);

            yield return null;
        }

        float holdTime = s.duration - scaleInTime - scaleOutTime;
        if (holdTime > 0f)
            yield return new WaitForSeconds(holdTime);

        float tOut = 0f;
        while (tOut < scaleOutTime)
        {
            tOut += Time.deltaTime;
            float a = (scaleOutTime <= 0f) ? 1f : Mathf.Clamp01(tOut / scaleOutTime);

            if (bubbleT) bubbleT.localScale = Vector3.LerpUnclamped(bubbleTarget, Vector3.zero, a);
            if (textT) textT.localScale = Vector3.LerpUnclamped(textTarget, Vector3.zero, a);

            yield return null;
        }

        if (s.bubble) s.bubble.SetActive(false);
        if (s.textObj) s.textObj.SetActive(false);
    }

    void HideAll()
    {
        for (int i = 0; i < steps.Length; i++)
        {
            DialogueStep s = steps[i];
            if (s == null) continue;

            if (s.bubble != null) s.bubble.SetActive(false);
            if (s.textObj != null) s.textObj.SetActive(false);
        }
    }
}
