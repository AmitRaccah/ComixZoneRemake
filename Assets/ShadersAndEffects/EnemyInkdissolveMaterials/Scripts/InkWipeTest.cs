using UnityEngine;

public class InkWipeTest : MonoBehaviour
{
    [Header("Wipe Settings")]
    [SerializeField] private float wipeDuration = 2f;
    [SerializeField] private bool autoPlay = true;
    
    [Header("Ink Settings")]
    [SerializeField] private float inkBandWidth = 2.0f;
    
    [Header("Drip Effect")]
    [SerializeField] private InkDripController dripController;
    
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    
    private float minY;
    private float maxY;
    
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        FindCharacterBounds();
        
        // Apply ink band width to all materials
        foreach (Renderer rend in renderers)
        {
            if (rend.material.HasProperty("_InkBandWidth"))
            {
                rend.material.SetFloat("_InkBandWidth", inkBandWidth);
            }
        }
        
        // DEBUG: Print bounds info
        Debug.Log($"=== CHARACTER BOUNDS ===");
        Debug.Log($"Full character: {minY:F2} to {maxY:F2}");
        
        foreach (Renderer rend in renderers)
        {
            if (rend.name.Contains("Leg") || rend.name.Contains("Feet") || rend.name.Contains("leg") || rend.name.Contains("feet"))
            {
                Debug.Log($"{rend.name}: Y range {rend.bounds.min.y:F2} to {rend.bounds.max.y:F2}");
            }
        }
        
        if (autoPlay)
        {
            StartWipe();
        }
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
        if (isWiping)
        {
            wipeProgress += Time.deltaTime / wipeDuration;
            float actualThreshold = Mathf.Lerp(minY, maxY, wipeProgress);
        
            // Update shader
            foreach (Renderer rend in renderers)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_WipeTheshold", actualThreshold);
                rend.SetPropertyBlock(propertyBlock);
            }
        
            // Update drip position AND control emission
            if (dripController != null)
            {
                dripController.UpdateDripPosition(actualThreshold);
            
                // Stop emitting drips when we're past 70% done (texture is mostly revealed)
                if (wipeProgress > 0.7f)
                {
                    dripController.StopEmission();
                }
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
    }
    
    public void StartWipe()
    {
        wipeProgress = 0f;
        isWiping = true;
    
        if (dripController != null)
        {
            dripController.StartDrips(minY);
        }
    }
}