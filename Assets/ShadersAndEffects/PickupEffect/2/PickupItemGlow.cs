using UnityEngine;

/// <summary>
/// Ultra-simple ground glow that WILL work
/// Creates a glowing quad under your item
/// </summary>
public class SuperSimpleGlow : MonoBehaviour
{
    [Header("Simple Settings")]
    public Color glowColor = Color.yellow;
    [Range(0.5f, 5f)] public float size = 2.5f;
    [Range(0f, 10f)] public float brightness = 3f;
    [Range(0f, 1f)] public float groundHeight = 0.1f;
    
    private GameObject glowQuad;
    private Material glowMat;
    private Texture2D glowTexture;
    
    void Start()
    {
        CreateGroundGlow();
    }
    
    void CreateGroundGlow()
    {
        // Create the glow texture procedurally
        glowTexture = CreateGlowTexture();
        
        // Create quad
        glowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        glowQuad.name = "GroundGlow";
        glowQuad.transform.parent = transform;
        
        // Position under item
        glowQuad.transform.localPosition = new Vector3(0, -transform.position.y + groundHeight, 0);
        glowQuad.transform.localRotation = Quaternion.Euler(90, 0, 0);
        glowQuad.transform.localScale = new Vector3(size, size, 1);
        
        // Remove collider
        Destroy(glowQuad.GetComponent<Collider>());
        
        // Setup material
        glowMat = new Material(Shader.Find("Unlit/Transparent"));
        glowMat.mainTexture = glowTexture;
        glowMat.color = glowColor * brightness;
        
        glowQuad.GetComponent<Renderer>().material = glowMat;
        glowQuad.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        
        Debug.Log($"SuperSimpleGlow: Created ground glow for {gameObject.name}");
    }
    
    Texture2D CreateGlowTexture()
    {
        int resolution = 256;
        Texture2D tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        
        float center = resolution * 0.5f;
        
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                // Distance from center
                float dx = (x - center) / center;
                float dy = (y - center) / center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                // Create halftone dot pattern
                float gridSize = 15f;
                float dotSize = 0.35f;
                
                // Grid position
                float gx = (x / gridSize) % 1f;
                float gy = (y / gridSize) % 1f;
                
                // Hexagonal offset
                if (Mathf.FloorToInt(y / gridSize) % 2 == 1)
                    gx = ((x + gridSize * 0.5f) / gridSize) % 1f;
                
                // Distance to grid center
                float gdx = gx - 0.5f;
                float gdy = gy - 0.5f;
                float gdist = Mathf.Sqrt(gdx * gdx + gdy * gdy);
                
                // Fade based on distance from center of whole texture
                float fade = 1f - Mathf.Clamp01(dist);
                fade = Mathf.Pow(fade, 0.8f);
                
                // Draw dot
                float dot = gdist < (dotSize * fade) ? 1f : 0f;
                dot = Mathf.SmoothStep(0f, 1f, (dotSize * fade - gdist) / 0.1f);
                
                // Apply radial fade
                float alpha = dot * fade;
                
                // Smooth falloff at edges
                if (dist > 0.9f)
                    alpha *= 1f - (dist - 0.9f) / 0.1f;
                
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        
        return tex;
    }
    
    void Update()
    {
        if (glowMat != null)
        {
            // Pulsing animation
            float pulse = Mathf.Sin(Time.time * 2f) * 0.2f + 1f;
            glowMat.color = glowColor * brightness * pulse;
        }
        
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
        
        if (glowTexture != null)
        {
            if (Application.isPlaying)
                Destroy(glowTexture);
            else
                DestroyImmediate(glowTexture);
        }
    }
}