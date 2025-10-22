using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[AddComponentMenu("UI/Effects/UI Icon Glow Blink")]
public class UIIconGlowBlink : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Image of the item icon. If empty, taken from this GameObject.")]
    [SerializeField] private Image targetImage;

    [Header("Timing")]
    [Tooltip("How often to blink (seconds).")]
    [SerializeField] private float blinkEverySeconds = 2f;
    [Tooltip("How long the blink pulse lasts (seconds).")]
    [SerializeField] private float pulseDuration = 0.2f;

    [Header("Strength")]
    [Tooltip("How much to add to _GlowAmount at pulse peak. Can be > 1 for very strong glow.")]
    [SerializeField, Min(0f)] private float pulseStrength = 2f;

    const string GLOW_AMOUNT_PROP = "_GlowAmount";

    Material runtimeMat;
    float baseGlowAmount;
    bool hasGlowProp;
    bool lastHadSprite;
    Coroutine loopCo;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        if (!targetImage) { enabled = false; return; }

        var sharedMat = targetImage.material;
        if (sharedMat == null)
        {
            Debug.LogWarning($"{nameof(UIIconGlowBlink)}: Image has no material. Assign a material using 'UI/SimpleGlow'.", this);
            enabled = false;
            return;
        }

        runtimeMat = new Material(sharedMat);
        targetImage.material = runtimeMat;

        hasGlowProp = runtimeMat.HasProperty(GLOW_AMOUNT_PROP);
        if (!hasGlowProp)
        {
            Debug.LogWarning($"{nameof(UIIconGlowBlink)}: Material is missing '{GLOW_AMOUNT_PROP}'. Use 'UI/SimpleGlow'.", this);
            enabled = false;
            return;
        }

        baseGlowAmount = runtimeMat.GetFloat(GLOW_AMOUNT_PROP);
        lastHadSprite = targetImage.enabled && targetImage.sprite != null;
    }

    void OnEnable()
    {
        if (!runtimeMat || !hasGlowProp) return;
        StopLoop();
        loopCo = StartCoroutine(BlinkLoop());
    }

    void OnDisable()
    {
        StopLoop();
        RestoreBaseGlow();
    }

    void OnDestroy()
    {
        if (runtimeMat)
        {
            if (Application.isPlaying) Destroy(runtimeMat);
            else DestroyImmediate(runtimeMat);
        }
    }

    IEnumerator BlinkLoop()
    {
        while (true)
        {
            if (!targetImage) yield break;

            bool hasSprite = targetImage.enabled && targetImage.sprite != null;

            if (hasSprite && !lastHadSprite)
            {
                baseGlowAmount = runtimeMat.GetFloat(GLOW_AMOUNT_PROP);
                lastHadSprite = true;
            }
            else if (!hasSprite && lastHadSprite)
            {
                RestoreBaseGlow();
                lastHadSprite = false;
            }

            if (!hasSprite)
            {
                yield return null;
                continue;
            }

            float waited = 0f;
            while (waited < blinkEverySeconds)
            {
                waited += Time.deltaTime;
                if (!(targetImage.enabled && targetImage.sprite != null))
                {
                    RestoreBaseGlow();
                    goto ContinueLoop;
                }
                yield return null;
            }

            float t = 0f;
            while (t < pulseDuration && targetImage.enabled && targetImage.sprite != null)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / pulseDuration);
                float s = Mathf.Sin(p * Mathf.PI); 

                float value = baseGlowAmount + s * pulseStrength; 
                runtimeMat.SetFloat(GLOW_AMOUNT_PROP, value);

                yield return null;
            }

            RestoreBaseGlow();

        ContinueLoop:
            yield return null;
        }
    }

    void RestoreBaseGlow()
    {
        if (runtimeMat && hasGlowProp)
            runtimeMat.SetFloat(GLOW_AMOUNT_PROP, baseGlowAmount);
    }

    void StopLoop()
    {
        if (loopCo != null) { StopCoroutine(loopCo); loopCo = null; }
    }
}
