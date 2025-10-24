using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InkWipeTest : MonoBehaviour
{
    [Header("Wipe Settings")]
    [SerializeField] private float wipeDuration = 2f;
    [SerializeField] private bool autoPlay = true;
    [SerializeField] private bool useManualRange = false;
    [SerializeField] private float manualMinY = -1f;
    [SerializeField] private float manualMaxY = 2f;

    [Header("Ink Settings")]
    [SerializeField] private float inkBandWidth = 0.39f;

    [Header("Splash Effect")]
    [SerializeField] private ParticleSystem splashEffectPrefab;
    [SerializeField] private float splashTriggerTime = 0.35f;
    [SerializeField] private Vector3 splashPositionOffset = new Vector3(0f, 0.64f, -0.2f);
    [SerializeField] private Vector3 splashRotation = Vector3.zero;
    [SerializeField] private float splashScale = 0.17f;

    [Header("Puddle Effect")]
    [SerializeField] private string puddleVfxId;
    [SerializeField] private float puddleDelay = 0.3f;
    [SerializeField] private Vector3 puddlePositionOffset = Vector3.zero;
    [SerializeField] private Vector3 puddleRotation = new Vector3(90f, 0.94f, 0f);
    [SerializeField] private float puddleScale = 0.44f;

    static readonly int PID_InkBandWidth = Shader.PropertyToID("_InkBandWidth");
    static readonly int PID_WipeOffset = Shader.PropertyToID("_WipeOffset");
    static readonly int PID_WipeMinY = Shader.PropertyToID("_WipeMinY");
    static readonly int PID_WipeMaxY = Shader.PropertyToID("_WipeMaxY");
    static readonly int PID_WipeTheshold = Shader.PropertyToID("_WipeTheshold");
    static readonly int PID_WipeThreshold = Shader.PropertyToID("_WipeThreshold");

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    private bool splashTriggered = false;
    private ParticleSystem spawnedSplash;
    private float minY;
    private float maxY;

    public float Duration => wipeDuration;

    void Awake()
    {
        if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
    }

    void Start()
    {
        RefreshRenderers(true);
        FindCharacterBounds();
        ApplyStaticPropsToAll(inkBandWidth, minY);
        if (autoPlay) StartWipe();
    }

    void RefreshRenderers(bool includeInactive)
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive);
        List<Renderer> valid = new List<Renderer>(renderers.Length);
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i] != null) valid.Add(renderers[i]);
        renderers = valid.ToArray();
    }

    void ApplyStaticPropsToAll(float bandWidth, float initialThresholdWorld)
    {
        float offset = transform.position.y;
        float thrRel = initialThresholdWorld - offset;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;

            propertyBlock.Clear();
            propertyBlock.SetFloat(PID_InkBandWidth, bandWidth);
            propertyBlock.SetFloat(PID_WipeOffset, offset);
            propertyBlock.SetFloat(PID_WipeMinY, minY - offset);
            propertyBlock.SetFloat(PID_WipeMaxY, maxY - offset);
            propertyBlock.SetFloat(PID_WipeTheshold, thrRel);
            propertyBlock.SetFloat(PID_WipeThreshold, thrRel);
            r.SetPropertyBlock(propertyBlock);
        }
    }

    void FindCharacterBounds()
    {
        if (useManualRange)
        {
            minY = transform.position.y + manualMinY;
            maxY = transform.position.y + manualMaxY;
            return;
        }

        if (renderers == null || renderers.Length == 0)
            RefreshRenderers(true);

        float min = float.PositiveInfinity;
        float max = float.NegativeInfinity;
        int counted = 0;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;

            var b = r.bounds;
            if (float.IsNaN(b.min.y) || float.IsNaN(b.max.y)) continue;
            if (b.size.sqrMagnitude < 1e-8f) continue;

            if (b.min.y < min) min = b.min.y;
            if (b.max.y > max) max = b.max.y;
            counted++;
        }

        if (counted == 0)
        {
            float y = transform.position.y;
            minY = y - 1f;
            maxY = y + 2f;
        }
        else
        {
            minY = min - 0.2f;
            maxY = max + 0.2f;
        }
    }

    void Update()
    {
        if (!isWiping) return;

        wipeProgress += Time.deltaTime / Mathf.Max(0.0001f, wipeDuration);
        float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);

        float offset = transform.position.y;
        float minRel = minY - offset;
        float maxRel = maxY - offset;
        float thrRel = actualThreshold - offset;

        for (int i = 0; i < renderers.Length; i++)
        {
            var r = renderers[i];
            if (!r) continue;

            propertyBlock.Clear();
            propertyBlock.SetFloat(PID_WipeOffset, offset);
            propertyBlock.SetFloat(PID_WipeMinY, minRel);
            propertyBlock.SetFloat(PID_WipeMaxY, maxRel);
            propertyBlock.SetFloat(PID_WipeTheshold, thrRel);
            propertyBlock.SetFloat(PID_WipeThreshold, thrRel);
            r.SetPropertyBlock(propertyBlock);
        }

        if (!splashTriggered && wipeProgress >= splashTriggerTime)
        {
            TriggerSplashAndPuddle();
            splashTriggered = true;
        }

        if (wipeProgress >= 1f)
            isWiping = false;
    }

    void TriggerSplashAndPuddle()
    {
        if (splashEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + splashPositionOffset;
            Quaternion spawnRotation = Quaternion.Euler(splashRotation);
            spawnedSplash = Instantiate(splashEffectPrefab, spawnPosition, spawnRotation);
            spawnedSplash.transform.localScale = Vector3.one * splashScale;
            spawnedSplash.Play();
        }

        if (!string.IsNullOrEmpty(puddleVfxId))
            StartCoroutine(SpawnPuddle());
    }

    IEnumerator SpawnPuddle()
    {
        yield return new WaitForSeconds(puddleDelay);

        if (VfxPoolManager.Instance != null)
        {
            Vector3 puddlePosition = transform.position + puddlePositionOffset;
            Quaternion puddleRot = Quaternion.Euler(puddleRotation);
            var go = VfxPoolManager.Instance.Spawn(puddleVfxId, puddlePosition, puddleRot);
            if (go) go.transform.localScale = Vector3.one * puddleScale;
        }
    }

    public void StartWipe()
    {
        RefreshRenderers(true);

        wipeProgress = 0f;
        isWiping = true;
        splashTriggered = false;

        FindCharacterBounds();
        ApplyStaticPropsToAll(inkBandWidth, minY);
    }

    void OnDisable()
    {
        if (spawnedSplash != null)
        {
            Destroy(spawnedSplash.gameObject);
            spawnedSplash = null;
        }
    }
}
