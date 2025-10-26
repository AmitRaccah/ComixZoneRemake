using UnityEngine;

/// <summary>
/// Complete pickup item glow effect - UPDATED VERSION
/// Creates a glowing quad under your item + rim glow on object
/// All controls work in real-time!
/// </summary>
public class PickupGlowEffect : MonoBehaviour
{
    [Header("Ground Glow Settings")]
    public Color glowColor = Color.yellow;
    [Range(0.5f, 5f)] public float size = 2.5f;
    [Range(0f, 10f)] public float brightness = 3f;
    [Range(0f, 1f)] public float groundHeight = 0.1f;
    [Range(0f, 360f)] public float rotation = 0f;
    
    [Header("Object Rim Glow")]
    [Range(0f, 3f)] public float rimIntensity = 0.5f;
    [Range(0.1f, 8f)] public float rimPower = 3f;
    
    private GameObject glowQuad;
    private Material glowMat;
    private Material objectMaterial;
    private Renderer itemRenderer;
    
    // Store last values to detect changes
    private Color lastColor;
    private float lastBrightness;
    private float lastGroundHeight;
    private float lastRotation;
    private Vector3 lastPosition;
    
    void Start()
    {
        CreateGroundGlow();
        SetupObjectRimGlow();
        
        // Store initial values
        lastColor = glowColor;
        lastBrightness = brightness;
        lastGroundHeight = groundHeight;
        lastRotation = rotation;
        lastPosition = transform.position;
    }
    
    void SetupObjectRimGlow()
    {
        itemRenderer = GetComponent<Renderer>();
        if (itemRenderer == null) return;
        
        // Try to use the rim glow shader
        Shader rimShader = Shader.Find("Custom/ItemHighlightGlow");
        
        if (rimShader != null)
        {
            objectMaterial = new Material(rimShader);
            
            // Copy existing texture and color from original material
            if (itemRenderer.sharedMaterial != null)
            {
                if (itemRenderer.sharedMaterial.mainTexture != null)
                {
                    objectMaterial.mainTexture = itemRenderer.sharedMaterial.mainTexture;
                }
                
                // Preserve original base color
                if (itemRenderer.sharedMaterial.HasProperty("_Color"))
                {
                    objectMaterial.SetColor("_Color", itemRenderer.sharedMaterial.GetColor("_Color"));
                }
                else
                {
                    objectMaterial.SetColor("_Color", Color.white);
                }
            }
            else
            {
                objectMaterial.SetColor("_Color", Color.white);
            }
            
            UpdateObjectMaterial();
            itemRenderer.material = objectMaterial;
            
            Debug.Log("PickupGlowEffect: Applied rim glow shader to object");
        }
        else
        {
            Debug.LogWarning("PickupGlowEffect: Rim glow shader not found, object will not have rim effect");
        }
    }
    
    void UpdateObjectMaterial()
    {
        if (objectMaterial != null)
        {
            objectMaterial.SetColor("_GlowColor", glowColor);
            objectMaterial.SetFloat("_GlowIntensity", rimIntensity);
            objectMaterial.SetFloat("_RimPower", rimPower);
            
            // Ensure base color stays white/original (not affected by glow color)
            // We only set this once in SetupObjectRimGlow now
        }
    }
    
    void CreateGroundGlow()
    {
        // Create quad
        glowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glowQuad.name = "GroundGlow";
        glowQuad.transform.SetParent(transform, false);
        
        // Position under item
        UpdateGroundPosition();
        UpdateGroundRotation();
        glowQuad.transform.localScale = new Vector3(size, size, 1);
        
        // Remove collider
        Destroy(glowQuad.GetComponent<Collider>());
        
        // Setup material with shader
        Shader groundShader = Shader.Find("Custom/BrightGroundGlow");
        
        if (groundShader == null)
        {
            Debug.LogWarning("BrightGroundGlow shader not found, trying SimpleGroundGlow...");
            groundShader = Shader.Find("Custom/SimpleGroundGlow");
        }
        
        if (groundShader == null)
        {
            Debug.LogError("No ground glow shader found! Please import BrightGroundGlow.shader");
            return;
        }
        
        glowMat = new Material(groundShader);
        UpdateGroundMaterial();
        
        glowQuad.GetComponent<Renderer>().material = glowMat;
        glowQuad.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        glowQuad.GetComponent<Renderer>().receiveShadows = false;
        
        Debug.Log($"PickupGlowEffect: Created ground glow for {gameObject.name} using shader: {groundShader.name}");
    }
    
    void UpdateGroundPosition()
    {
        if (glowQuad != null)
        {
            Vector3 pos = transform.position;
            pos.y = groundHeight;
            glowQuad.transform.position = pos;
        }
    }
    
    void UpdateGroundRotation()
    {
        if (glowQuad != null)
        {
            // Face up (90 degrees on X) + custom rotation on Y axis
            glowQuad.transform.rotation = Quaternion.Euler(90, rotation, 0);
        }
    }
    
    void UpdateGroundMaterial()
    {
        if (glowMat != null)
        {
            glowMat.SetColor("_Color", glowColor);
            glowMat.SetFloat("_Intensity", brightness);
            glowMat.SetFloat("_Scale", 20f); // Dot density
            glowMat.SetFloat("_Speed", 2f); // Pulse speed
        }
    }
    
    Texture2D CreateGlowTexture()
    {
        // Not used anymore - keeping for compatibility
        return null;
    }
    
    void Update()
    {
        bool needsUpdate = false;
        
        // Check if color changed
        if (glowColor != lastColor)
        {
            lastColor = glowColor;
            needsUpdate = true;
            UpdateObjectMaterial();
        }
        
        // Check if brightness changed
        if (brightness != lastBrightness)
        {
            lastBrightness = brightness;
            needsUpdate = true;
        }
        
        // Check if ground height changed
        if (groundHeight != lastGroundHeight)
        {
            lastGroundHeight = groundHeight;
            UpdateGroundPosition();
        }
        
        // Check if rotation changed
        if (rotation != lastRotation)
        {
            lastRotation = rotation;
            UpdateGroundRotation();
        }
        
        // Check if position changed
        if (transform.position != lastPosition)
        {
            lastPosition = transform.position;
            UpdateGroundPosition();
        }
        
        // Update ground material if needed (shader handles pulsing)
        if (needsUpdate)
        {
            UpdateGroundMaterial();
        }
        
        // Update object rim glow
        UpdateObjectMaterial();
        
        // Update size
        if (glowQuad != null)
        {
            glowQuad.transform.localScale = new Vector3(size, size, 1);
        }
    }
    
    void OnDestroy()
    {
        if (glowQuad != null)
        {
            if (Application.isPlaying)
                Destroy(glowQuad);
            else
                DestroyImmediate(glowQuad);
        }
        
        if (glowMat != null)
        {
            if (Application.isPlaying)
                Destroy(glowMat);
            else
                DestroyImmediate(glowMat);
        }
        
        if (objectMaterial != null)
        {
            if (Application.isPlaying)
                Destroy(objectMaterial);
            else
                DestroyImmediate(objectMaterial);
        }
    }
}