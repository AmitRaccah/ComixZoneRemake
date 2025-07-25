using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class PencilDissolveAnimator : MonoBehaviour
{
    [Tooltip("The material instance using your PencilDissolve shader")]
    public Material pencilMat;
    
    [Tooltip("How long (seconds) the noise dissolve takes")]
    public float dissolveDuration = 1.5f;
    [Tooltip("How long (seconds) the wipe takes")]
    public float wipeDuration = 1.5f;

    // Shader property names
    static readonly int _DissolveThreshold = Shader.PropertyToID("_DissolveThreshold");
    static readonly int _WipeThreshold    = Shader.PropertyToID("_WipeThreshold");

    void Awake()
    {
        // If you forgot to assign the material, pull it from this object's renderer
        if (pencilMat == null)
        {
            pencilMat = GetComponent<Renderer>().material;
        }
        // Start with everything hidden
        pencilMat.SetFloat(_DissolveThreshold, 1f);
        pencilMat.SetFloat(_WipeThreshold, 1f);
    }

    void OnEnable()
    {
        StartCoroutine(AnimateSketchIn());
    }

    IEnumerator AnimateSketchIn()
    {
        float t = 0f;
        // Phase 1: noise‑based pencil dissolve
        while (t < dissolveDuration)
        {
            float d = Mathf.Lerp(1f, 0f, t / dissolveDuration);
            pencilMat.SetFloat(_DissolveThreshold, d);
            t += Time.deltaTime;
            yield return null;
        }
        pencilMat.SetFloat(_DissolveThreshold, 0f);

        // Reset timer for wipe
        t = 0f;
        // Phase 2: bottom‑to‑top wipe
        while (t < wipeDuration)
        {
            float w = Mathf.Lerp(1f, 0f, t / wipeDuration);
            pencilMat.SetFloat(_WipeThreshold, w);
            t += Time.deltaTime;
            yield return null;
        }
        pencilMat.SetFloat(_WipeThreshold, 0f);
    }
}