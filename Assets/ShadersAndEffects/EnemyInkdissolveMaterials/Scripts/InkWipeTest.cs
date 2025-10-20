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
    private MaterialPropertyBlock propertyBlock;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    private bool splashTriggered = false;
    private ParticleSystem spawnedSplash;
    
    private float minY;
    private float maxY;
    
    void Start()
    {
        // Find all renderers in children (all body parts)
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
    
        // Calculate character bounds for proper top-to-bottom wipe
        FindCharacterBounds();
    
        // FORCE reset all materials to start at minimum (fully inked) BEFORE anything else!
        foreach (Renderer rend in renderers)
        {
            // Set ink band width
            if (rend.material.HasProperty("_InkBandWidth"))
            {
                rend.material.SetFloat("_InkBandWidth", inkBandWidth);
            }
        
            // CRITICAL: Reset to start position (bottom = fully inked)
            if (rend.material.HasProperty("_WipeTheshold"))
            {
                rend.material.SetFloat("_WipeTheshold", minY);
                Debug.Log($"Reset {rend.name} to minY: {minY:F2}");
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
            // Make manual values relative to character position
            minY = transform.position.y + manualMinY;
            maxY = transform.position.y + manualMaxY;
            Debug.Log($"Using MANUAL bounds (relative to character at Y={transform.position.y:F2}): {minY:F2} to {maxY:F2}");
            return;
        }
        
        // Auto-calculate from renderers
        minY = float.MaxValue;
        maxY = float.MinValue;
        
        Debug.Log("=== FINDING BOUNDS ===");
        foreach (Renderer rend in renderers)
        {
            Bounds bounds = rend.bounds;
            Debug.Log($"Renderer: {rend.name}, Y range: {bounds.min.y:F2} to {bounds.max.y:F2}");
            
            if (bounds.min.y < minY) minY = bounds.min.y;
            if (bounds.max.y > maxY) maxY = bounds.max.y;
        }
        
        Debug.Log($"Auto-calculated bounds BEFORE padding: {minY:F2} to {maxY:F2}");
        
        // Add padding
        minY -= 0.2f;
        maxY += 0.2f;
        
        Debug.Log($"Auto-calculated bounds AFTER padding: {minY:F2} to {maxY:F2}");
    }
    
    void Update()
    {
        if (isWiping)
        {
            // Update wipe progress
            wipeProgress += Time.deltaTime / wipeDuration;
            float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);
            
            // Apply wipe threshold to all body part materials
            foreach (Renderer rend in renderers)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_WipeTheshold", actualThreshold);
                rend.SetPropertyBlock(propertyBlock);
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