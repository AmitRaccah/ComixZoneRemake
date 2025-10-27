using UnityEngine;
using UnityEngine.Video;

/// <summary>
/// Controls the emission and brightness of a video screen
/// Perfect for in-game monitors, TVs, and displays
/// Add this to the Quad/mesh that displays the video
/// </summary>
public class VideoScreenController : MonoBehaviour
{
    [Header("Screen Appearance")]
    [Tooltip("Base brightness of the screen (0-2)")]
    [Range(0f, 2f)]
    public float brightness = 1f;
    
    [Tooltip("Emission intensity - makes the screen glow (0-10)")]
    [Range(0f, 10f)]
    public float emissionIntensity = 2f;
    
    [Tooltip("Color tint for the screen")]
    public Color screenTint = Color.white;
    
    [Header("Emission Settings")]
    [Tooltip("Enable HDR emission for very bright glows")]
    public bool useHDREmission = true;
    
    [Tooltip("Emission color (multiplied by video content)")]
    public Color emissionColor = Color.white;
    
    [Header("Advanced")]
    [Tooltip("Auto-apply changes in editor")]
    public bool liveUpdate = true;
    
    private Renderer screenRenderer;
    private Material screenMaterial;
    private VideoPlayer videoPlayer;
    
    // Store original values
    private Color originalColor;
    private Color originalEmission;
    
    void Start()
    {
        SetupScreen();
    }
    
    void SetupScreen()
    {
        // Try to find video player - check self, parent, and siblings
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null && transform.parent != null)
        {
            videoPlayer = transform.parent.GetComponentInChildren<VideoPlayer>();
        }
        if (videoPlayer == null)
        {
            videoPlayer = FindObjectOfType<VideoPlayer>();
        }
        
        if (videoPlayer != null)
        {
            Debug.Log($"VideoScreenController: Found VideoPlayer on {videoPlayer.gameObject.name}");
        }
        
        // Get renderer (should be on this object - the Quad)
        screenRenderer = GetComponent<Renderer>();
        if (screenRenderer == null)
        {
            Debug.LogError("VideoScreenController: No Renderer found on " + gameObject.name);
            return;
        }
        
        // Create material instance to avoid affecting other objects
        screenMaterial = screenRenderer.material;
        
        // Store original values
        if (screenMaterial.HasProperty("_Color"))
            originalColor = screenMaterial.GetColor("_Color");
        
        if (screenMaterial.HasProperty("_EmissionColor"))
            originalEmission = screenMaterial.GetColor("_EmissionColor");
        
        // Apply initial settings
        UpdateScreenAppearance();
        
        Debug.Log($"VideoScreenController: Setup complete for {gameObject.name}");
    }
    
    void Update()
    {
        if (liveUpdate && screenMaterial != null)
        {
            UpdateScreenAppearance();
        }
    }
    
    void UpdateScreenAppearance()
    {
        if (screenMaterial == null) return;
        
        // Check if using custom VideoScreenEmissive shader
        bool isCustomShader = screenMaterial.shader.name.Contains("VideoScreenEmissive");
        
        if (isCustomShader)
        {
            // Custom shader properties
            if (screenMaterial.HasProperty("_Brightness"))
            {
                screenMaterial.SetFloat("_Brightness", brightness);
            }
            
            if (screenMaterial.HasProperty("_EmissionStrength"))
            {
                screenMaterial.SetFloat("_EmissionStrength", emissionIntensity);
            }
            
            if (screenMaterial.HasProperty("_Color"))
            {
                screenMaterial.SetColor("_Color", screenTint);
            }
        }
        else
        {
            // Standard shader properties
            // Update base color/brightness
            if (screenMaterial.HasProperty("_Color"))
            {
                Color finalColor = screenTint * brightness;
                screenMaterial.SetColor("_Color", finalColor);
            }
            
            // Update emission
            if (screenMaterial.HasProperty("_EmissionColor"))
            {
                Color finalEmission;
                
                if (useHDREmission)
                {
                    // HDR emission for bright glows
                    finalEmission = emissionColor * Mathf.Pow(2f, emissionIntensity);
                }
                else
                {
                    // Standard emission
                    finalEmission = emissionColor * emissionIntensity;
                }
                
                screenMaterial.SetColor("_EmissionColor", finalEmission);
            }
            
            // Enable emission keyword for standard shader
            screenMaterial.EnableKeyword("_EMISSION");
        }
        
        // Update global illumination
        if (screenRenderer != null)
        {
            screenRenderer.UpdateGIMaterials();
        }
    }
    
    // Public methods to control from code
    
    public void SetBrightness(float value)
    {
        brightness = Mathf.Clamp(value, 0f, 2f);
        UpdateScreenAppearance();
    }
    
    public void SetEmission(float value)
    {
        emissionIntensity = Mathf.Clamp(value, 0f, 10f);
        UpdateScreenAppearance();
    }
    
    public void SetScreenTint(Color color)
    {
        screenTint = color;
        UpdateScreenAppearance();
    }
    
    public void SetEmissionColor(Color color)
    {
        emissionColor = color;
        UpdateScreenAppearance();
    }
    
    // Presets
    
    public void ApplyPresetOff()
    {
        brightness = 0.1f;
        emissionIntensity = 0f;
        UpdateScreenAppearance();
    }
    
    public void ApplyPresetNormal()
    {
        brightness = 1f;
        emissionIntensity = 2f;
        screenTint = Color.white;
        emissionColor = Color.white;
        UpdateScreenAppearance();
    }
    
    public void ApplyPresetBright()
    {
        brightness = 1.5f;
        emissionIntensity = 5f;
        useHDREmission = true;
        UpdateScreenAppearance();
    }
    
    public void ApplyPresetDim()
    {
        brightness = 0.5f;
        emissionIntensity = 1f;
        UpdateScreenAppearance();
    }
    
    // Smooth transitions
    
    public void FadeIn(float duration)
    {
        StartCoroutine(FadeRoutine(0f, 1f, 0f, emissionIntensity, duration));
    }
    
    public void FadeOut(float duration)
    {
        StartCoroutine(FadeRoutine(brightness, 0.1f, emissionIntensity, 0f, duration));
    }
    
    private System.Collections.IEnumerator FadeRoutine(float startBrightness, float endBrightness, float startEmission, float endEmission, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            brightness = Mathf.Lerp(startBrightness, endBrightness, t);
            emissionIntensity = Mathf.Lerp(startEmission, endEmission, t);
            
            UpdateScreenAppearance();
            
            yield return null;
        }
        
        brightness = endBrightness;
        emissionIntensity = endEmission;
        UpdateScreenAppearance();
    }
    
    // Flickering effect
    
    public void StartFlicker(float flickerSpeed = 10f, float flickerAmount = 0.3f)
    {
        StartCoroutine(FlickerRoutine(flickerSpeed, flickerAmount));
    }
    
    private System.Collections.IEnumerator FlickerRoutine(float speed, float amount)
    {
        float baseBrightness = brightness;
        float baseEmission = emissionIntensity;
        
        while (true)
        {
            float flicker = Mathf.PerlinNoise(Time.time * speed, 0f) * 2f - 1f;
            
            brightness = baseBrightness + flicker * amount;
            emissionIntensity = baseEmission + flicker * amount * 2f;
            
            UpdateScreenAppearance();
            
            yield return null;
        }
    }
    
    void OnDestroy()
    {
        // Clean up material instance
        if (screenMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(screenMaterial);
            else
                DestroyImmediate(screenMaterial);
        }
    }
    
    void OnValidate()
    {
        // Update in editor when values change
        if (screenMaterial != null && liveUpdate)
        {
            UpdateScreenAppearance();
        }
    }
}