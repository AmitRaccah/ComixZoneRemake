using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the ink wipe reveal effect for enemies with splash and puddle effects
/// Attach to the parent enemy GameObject that contains all body part renderers
/// </summary>
public class InkWipeTest : MonoBehaviour
{
    [Header("Wipe Settings")]
    [Tooltip("How long the entire wipe effect takes")]
    [SerializeField] private float wipeDuration = 2f;
    
    [Tooltip("Start wipe automatically on spawn")]
    [SerializeField] private bool autoPlay = true;
    
    [Tooltip("Use manual Y range instead of auto-calculating from bounds")]
    [SerializeField] private bool useManualRange = false;
    
    [Tooltip("Manual start Y position (relative to character)")]
    [SerializeField] private float manualMinY = -1f;
    
    [Tooltip("Manual end Y position (relative to character)")]
    [SerializeField] private float manualMaxY = 2f;
    
    [Header("Ink Settings")]
    [Tooltip("Width of the ink band between transparent and revealed areas")]
    [SerializeField] private float inkBandWidth = 0.39f;
    
    [Header("Splash Effect")]
    [Tooltip("Particle system prefab for the splash (e.g., BloodSplashBig)")]
    [SerializeField] private ParticleSystem splashEffectPrefab;
    
    [Tooltip("When to trigger splash (0-1): 0=start, 0.5=middle, 1=end")]
    [SerializeField] private float splashTriggerTime = 0.35f;
    
    [Tooltip("Offset from enemy position for splash spawn")]
    [SerializeField] private Vector3 splashPositionOffset = new Vector3(0f, 0.64f, -0.2f);
    
    [Tooltip("Rotation of splash effect in degrees (X, Y, Z)")]
    [SerializeField] private Vector3 splashRotation = Vector3.zero;
    
    [Tooltip("Scale of splash effect (uniform multiplier)")]
    [SerializeField] private float splashScale = 0.17f;
    
    [Header("Puddle Effect")]
    [Tooltip("Particle system prefab for ground puddle (e.g., ink_Spots)")]
    [SerializeField] private GameObject puddlePrefab;
    
    [Tooltip("Delay in seconds after splash before puddle appears")]
    [SerializeField] private float puddleDelay = 0.3f;
    
    [Tooltip("How long puddle stays visible before being destroyed")]
    [SerializeField] private float puddleDuration = 6f;
    
    [Tooltip("Offset from enemy position for puddle spawn (usually at ground level)")]
    [SerializeField] private Vector3 puddlePositionOffset = Vector3.zero;
    
    [Tooltip("Rotation of puddle in degrees (X=90 for flat on ground)")]
    [SerializeField] private Vector3 puddleRotation = new Vector3(90f, 0.94f, 0f);
    
    [Tooltip("Uniform scale multiplier for puddle size")]
    [SerializeField] private float puddleScale = 0.44f;
    
    // Private variables
    private Renderer[] renderers;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    private bool splashTriggered = false;
    private ParticleSystem spawnedSplash;
    
    private float minY;
    private float maxY;
    
    void Start()
    {
        Debug.Log($"[{gameObject.name}] Creating material instances...");
        
        // Find all renderers in children (all body parts)
        renderers = GetComponentsInChildren<Renderer>();
    
        Debug.Log($"[{gameObject.name}] Found {renderers.Length} renderers!");
    
        // CREATE UNIQUE MATERIAL INSTANCES for this enemy
        foreach (Renderer rend in renderers)
        {
            if (rend.material.HasProperty("_InkBandWidth"))
            {
                rend.material.SetFloat("_InkBandWidth", inkBandWidth);
            }
    
            if (rend.material.HasProperty("_WipeTheshold"))
            {
                // Set the initial threshold to the RELATIVE minY
                rend.material.SetFloat("_WipeTheshold", minY); 
                Debug.Log($"[{gameObject.name}] Reset {rend.name} _WipeTheshold to {minY:F2} (Relative)");
            }
        }
    
        // Calculate character bounds
        FindCharacterBounds();
    
        // Apply settings and reset
        foreach (Renderer rend in renderers)
        {
            if (rend.material.HasProperty("_InkBandWidth"))
            {
                rend.material.SetFloat("_InkBandWidth", inkBandWidth);
            }
        
            if (rend.material.HasProperty("_WipeTheshold"))
            {
                rend.material.SetFloat("_WipeTheshold", minY);
                Debug.Log($"[{gameObject.name}] Reset {rend.name} _WipeTheshold to {minY:F2}");
            }
        }
    
        if (autoPlay)
        {
            StartWipe();
        }
    }
    
    /// <summary>
    /// Calculates the Y bounds of the character for proper wipe range
    /// </summary>
    void FindCharacterBounds()
    {
        if (useManualRange)
        {
            // Keep manualMinY and manualMaxY as the RELATIVE bounds
            minY = manualMinY;
            maxY = manualMaxY;
            Debug.Log($"Using MANUAL bounds (Relative): {minY:F2} to {maxY:F2}");
            return;
        }

        // Auto-calculate RELATIVE to character
        float relativeMinY = float.MaxValue;
        float relativeMaxY = float.MinValue;

        Debug.Log("=== FINDING BOUNDS ===");
        foreach (Renderer rend in renderers)
        {
            Bounds bounds = rend.bounds;
    
            // Calculate RELATIVE to character position
            float rendererMinY = bounds.min.y - transform.position.y;
            float rendererMaxY = bounds.max.y - transform.position.y;
    
            Debug.Log($"Renderer: {rend.name}, Relative Y: {rendererMinY:F2} to {rendererMaxY:F2}");
    
            if (rendererMinY < relativeMinY) relativeMinY = rendererMinY;
            if (rendererMaxY > relativeMaxY) relativeMaxY = rendererMaxY;
        }

        Debug.Log($"Relative bounds: {relativeMinY:F2} to {relativeMaxY:F2}");

        // Add padding
        relativeMinY -= 0.2f;
        relativeMaxY += 0.2f;

        // Assign the RELATIVE bounds to minY and maxY
        minY = relativeMinY;
        maxY = relativeMaxY;

        Debug.Log($"Final RELATIVE bounds: {minY:F2} to {maxY:F2}");
    }
    
    void Update()
    {
        if (isWiping)
        {
            // Update wipe progress
            wipeProgress += Time.deltaTime / wipeDuration;
        
            // actualThreshold is the calculated RELATIVE Y offset
            float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);
    
            Debug.Log($"[{gameObject.name}] Wipe Progress: {wipeProgress:F2}, Relative Threshold: {actualThreshold:F2}");
    
            // Apply to material instances
            foreach (Renderer rend in renderers)
            {
                if (rend.material.HasProperty("_WipeTheshold"))
                {
                    // Pass the RELATIVE Y threshold
                    rend.material.SetFloat("_WipeTheshold", actualThreshold);
                }
                // *** NOTE: The code to set _WipeOffset has been removed ***
                // *** because the shader should now use Object Space.    ***
            }
    
            // Trigger splash and puddle at specified time
            if (!splashTriggered && wipeProgress >= splashTriggerTime)
            {
                TriggerSplashAndPuddle();
                splashTriggered = true;
            }
    
            // Finish wipe
            if (wipeProgress >= 1f)
            {
                isWiping = false;
            }
        }
    }
    
    /// <summary>
    /// Triggers both the splash particle effect and queues the puddle spawn
    /// </summary>
    void TriggerSplashAndPuddle()
    {
        // Spawn splash with custom position, rotation, and scale
        if (splashEffectPrefab != null)
        {
            Vector3 spawnPosition = transform.position + splashPositionOffset;
            Quaternion spawnRotation = Quaternion.Euler(splashRotation);
            spawnedSplash = Instantiate(splashEffectPrefab, spawnPosition, spawnRotation);
            spawnedSplash.transform.localScale = Vector3.one * splashScale;
            spawnedSplash.Play();
        }
        
        // Spawn puddle after delay
        if (puddlePrefab != null)
        {
            StartCoroutine(SpawnPuddle());
        }
    }
    
    /// <summary>
    /// Spawns the puddle effect after a delay and destroys it after duration
    /// </summary>
    IEnumerator SpawnPuddle()
    {
        // Wait for splash to happen first
        yield return new WaitForSeconds(puddleDelay);
        
        // Spawn puddle with custom position and rotation
        Vector3 puddlePosition = transform.position + puddlePositionOffset;
        Quaternion puddleRot = Quaternion.Euler(puddleRotation);
        GameObject puddle = Instantiate(puddlePrefab, puddlePosition, puddleRot);
        puddle.transform.localScale = Vector3.one * puddleScale;
        puddle.SetActive(true);
        
        // Destroy after duration
        Destroy(puddle, puddleDuration);
    }
    
    /// <summary>
    /// Public method to manually start the wipe effect
    /// </summary>
    public void StartWipe()
    {
        wipeProgress = 0f;
        isWiping = true;
        splashTriggered = false;
    }
}