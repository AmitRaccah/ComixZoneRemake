using UnityEngine;
using System.Collections;

public class InkWipeTestSimple : MonoBehaviour
{
    [Header("Wipe Settings")]
    [SerializeField] private float wipeDuration = 2f;
    [SerializeField] private bool autoPlay = true;
    
    [Header("Y Range Settings")]
    [SerializeField] private bool useManualRange = false;
    [SerializeField] private float manualMinY = -1f;
    [SerializeField] private float manualMaxY = 2f;
    
    [Header("Drip Effect")]
    [SerializeField] private InkDripController dripController;
    
    [Header("Splash Effect")]
    [SerializeField] private ParticleSystem splashEffectPrefab;
    [SerializeField] private float splashTriggerTime = 0.35f;
    [SerializeField] private Vector3 splashPositionOffset = new Vector3(0f, 0.64f, -0.2f);
    [SerializeField] private Vector3 splashRotation = Vector3.zero;
    [SerializeField] private float splashScale = 0.17f;
    
    [Header("Puddle Effect")]
    [SerializeField] private ParticleSystem puddleEffectPrefab;
    [SerializeField] private float puddleDelay = 0.3f;
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
    
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        if (useManualRange)
        {
            minY = manualMinY;
            maxY = manualMaxY;
            Debug.Log($"Using MANUAL range: {minY:F2} to {maxY:F2}");
        }
        else
        {
            FindCharacterBounds();
            Debug.Log($"Using AUTO bounds: {minY:F2} to {maxY:F2}");
        }
        
        // Set the shader's Y range to match this object!
        SetShaderYRange();
        
        if (autoPlay)
        {
            StartWipe();
        }
    }
    
    void SetShaderYRange()
    {
        foreach (Renderer rend in renderers)
        {
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_WipeMinY", minY);
            propertyBlock.SetFloat("_WipeMaxY", maxY);
            rend.SetPropertyBlock(propertyBlock);
        }
        Debug.Log($"Set shader _WipeMinY={minY:F2}, _WipeMaxY={maxY:F2}");
    }
    
    void FindCharacterBounds()
    {
        minY = float.MaxValue;
        maxY = float.MinValue;
        
        foreach (Renderer rend in renderers)
        {
            Bounds bounds = rend.bounds;
            if (bounds.min.y < minY) minY = bounds.min.y;
            if (bounds.max.y > maxY) maxY = bounds.max.y;
        }
        
        // Add padding
        minY -= 0.2f;
        maxY += 0.2f;
    }
    
    void Update()
    {
        if (!isWiping) return;
        
        wipeProgress += Time.deltaTime / wipeDuration;
        
        // Remap 0-1 to actual Y range
        float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);
        
        foreach (Renderer rend in renderers)
        {
            rend.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat("_WipeTheshold", actualThreshold);  // Note: matches your shader's spelling
            rend.SetPropertyBlock(propertyBlock);
        }
        
        // Update drip position
        if (dripController != null)
        {
            dripController.UpdateDripPosition(actualThreshold);
            
            // Stop emitting drips when 70% done
            if (wipeProgress > 0.7f)
            {
                dripController.StopEmission();
            }
        }
        
        // Trigger splash at specific time
        if (!splashTriggered && wipeProgress >= splashTriggerTime)
        {
            TriggerSplashAndPuddle();
            splashTriggered = true;
        }
        
        if (wipeProgress >= 1f)
        {
            isWiping = false;
            if (dripController != null)
            {
                dripController.StopDrips();
            }
        }
    }
    
    void TriggerSplashAndPuddle()
    {
        // Spawn splash effect
        if (splashEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + splashPositionOffset;
            Quaternion spawnRotation = Quaternion.Euler(splashRotation);
            spawnedSplash = Instantiate(splashEffectPrefab, spawnPosition, spawnRotation);
            spawnedSplash.transform.localScale = Vector3.one * splashScale;
            spawnedSplash.Play();
        }
        
        // Spawn puddle effect (delayed)
        if (puddleEffectPrefab != null)
        {
            StartCoroutine(SpawnPuddle());
        }
    }
    
    IEnumerator SpawnPuddle()
    {
        yield return new WaitForSeconds(puddleDelay);
        
        Vector3 puddlePosition = transform.position + puddlePositionOffset;
        Quaternion puddleRot = Quaternion.Euler(puddleRotation);
        ParticleSystem puddle = Instantiate(puddleEffectPrefab, puddlePosition, puddleRot);
        puddle.transform.localScale = Vector3.one * puddleScale;
        puddle.Play();
    }
    
    public void StartWipe()
    {
        wipeProgress = 0f;
        isWiping = true;
        splashTriggered = false;
        
        // Make sure shader knows the Y range
        SetShaderYRange();
        
        if (dripController != null)
        {
            dripController.StartDrips(minY);
        }
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