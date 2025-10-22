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
    private InkDripController dripController;
    
    void Start()
    {
        Debug.Log($"[{gameObject.name}] Creating material instances...");
    
        // Find all renderers in children (all body parts)
        renderers = GetComponentsInChildren<Renderer>();

        Debug.Log($"[{gameObject.name}] Found {renderers.Length} renderers!");

        // CREATE UNIQUE MATERIAL INSTANCES for this enemy
        foreach (Renderer rend in renderers)
        {
            Material oldMat = rend.sharedMaterial;
            // Use Material constructor to create a fresh instance
            rend.material = new Material(rend.sharedMaterial); 
            Debug.Log($"[{gameObject.name}] {rend.name}: Old={oldMat.GetInstanceID()}, New={rend.material.GetInstanceID()}");
        }

        // Calculate character bounds (minY/maxY are now RELATIVE)
        FindCharacterBounds();
    
        // Get the Drip Controller reference
        dripController = GetComponentInChildren<InkDripController>();

        // Apply settings and reset
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
            // Store relative offsets (from inspector)
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

        // Add padding and store the RELATIVE offsets
        relativeMinY -= 0.2f;
        relativeMaxY += 0.2f;
    
        // Store as RELATIVE OFFSETS
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
        
        // 1. Calculate the RELATIVE Y threshold for the SHADER
        // This is a RELATIVE offset (e.g., -1.7 to 1.7)
        float relativeThreshold = Mathf.Lerp(minY, maxY, wipeProgress);
        
        // 2. Calculate the WORLD Y position for the DRIP CONTROLLER
        // World Y = Character World Y + Relative Threshold
        float worldThresholdY = transform.position.y + relativeThreshold;
        
        Debug.Log($"[{gameObject.name}] Wipe Progress: {wipeProgress:F2}, Relative Threshold: {relativeThreshold:F2}, World Y: {worldThresholdY:F2}");
        
        // Apply dynamic values to material instances
        foreach (Renderer rend in renderers)
        {
            // Pass the RELATIVE Y threshold to preserve the smooth wipe
            if (rend.material.HasProperty("_WipeTheshold"))
            {
                rend.material.SetFloat("_WipeTheshold", relativeThreshold);
            }
            
            // Pass the character's full World Position to normalize the coordinates in the shader
            if (rend.material.HasProperty("_WipeOffset"))
            {
                rend.material.SetVector("_WipeOffset", transform.position);
            }
        }

        // 3. Update the drip controller with the World Y position
        if (dripController != null)
        {
            dripController.UpdateDripPosition(worldThresholdY);
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
            // Stop drips when the wipe finishes
            if (dripController != null)
            {
                dripController.StopEmission();
            }
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