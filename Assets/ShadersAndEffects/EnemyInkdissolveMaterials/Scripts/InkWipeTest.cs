using UnityEngine;
using System.Collections;

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
    [SerializeField] private GameObject puddlePrefab;
    [SerializeField] private float puddleDelay = 0.3f;
    [SerializeField] private float puddleDuration = 6f;
    [SerializeField] private Vector3 puddlePositionOffset = Vector3.zero;
    [SerializeField] private Vector3 puddleRotation = new Vector3(90f, 0.94f, 0f);
    [SerializeField] private float puddleScale = 0.44f;

    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    private bool splashTriggered = false;
    private ParticleSystem spawnedSplash;
    private float minY;
    private float maxY;

    public float Duration => wipeDuration;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();

        FindCharacterBounds();

        foreach (Renderer rend in renderers)
        {
            rend.GetPropertyBlock(propertyBlock);
            if (rend.material.HasProperty("_InkBandWidth"))
            {
                propertyBlock.SetFloat("_InkBandWidth", inkBandWidth);
            }
            if (rend.material.HasProperty("_WipeTheshold"))
            {
                propertyBlock.SetFloat("_WipeTheshold", minY);
            }
            rend.SetPropertyBlock(propertyBlock);
        }

        if (autoPlay)
        {
            StartWipe();
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

        minY = float.MaxValue;
        maxY = float.MinValue;

        foreach (Renderer rend in renderers)
        {
            Bounds bounds = rend.bounds;
            if (bounds.min.y < minY) minY = bounds.min.y;
            if (bounds.max.y > maxY) maxY = bounds.max.y;
        }

        minY -= 0.2f;
        maxY += 0.2f;
    }

    void Update()
    {
        if (isWiping)
        {
            wipeProgress += Time.deltaTime / wipeDuration;
            float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);

            foreach (Renderer rend in renderers)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_WipeOffset", transform.position.y);
                propertyBlock.SetFloat("_WipeMinY", minY - transform.position.y);
                propertyBlock.SetFloat("_WipeMaxY", maxY - transform.position.y);
                propertyBlock.SetFloat("_WipeTheshold", actualThreshold - transform.position.y);
                rend.SetPropertyBlock(propertyBlock);
            }

            if (!splashTriggered && wipeProgress >= splashTriggerTime)
            {
                TriggerSplashAndPuddle();
                splashTriggered = true;
            }

            if (wipeProgress >= 1f)
            {
                isWiping = false;
            }
        }
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

        if (puddlePrefab != null)
        {
            StartCoroutine(SpawnPuddle());
        }
    }

    IEnumerator SpawnPuddle()
    {
        yield return new WaitForSeconds(puddleDelay);

        Vector3 puddlePosition = transform.position + puddlePositionOffset;
        Quaternion puddleRot = Quaternion.Euler(puddleRotation);
        GameObject puddle = Instantiate(puddlePrefab, puddlePosition, puddleRot);
        puddle.transform.localScale = Vector3.one * puddleScale;
        puddle.SetActive(true);

        Destroy(puddle, puddleDuration);
    }

    public void StartWipe()
    {
        wipeProgress = 0f;
        isWiping = true;
        splashTriggered = false;
    }
}
