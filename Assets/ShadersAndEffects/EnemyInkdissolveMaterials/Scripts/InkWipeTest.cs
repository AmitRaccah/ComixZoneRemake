using UnityEngine;

public class InkWipeTest : MonoBehaviour
{
    [Header("Wipe Settings")]
    [SerializeField] private float wipeDuration = 2f;  // How long the wipe takes
    [SerializeField] private bool autoPlay = true;      // Start automatically
    
    private Renderer[] renderers;
    private MaterialPropertyBlock propertyBlock;
    private float wipeProgress = 0f;
    private bool isWiping = false;
    
    void Start()
    {
        // Find all renderers in children (for your 30 parts)
        renderers = GetComponentsInChildren<Renderer>();
        propertyBlock = new MaterialPropertyBlock();
        
        if (autoPlay)
        {
            StartWipe();
        }
    }
    
    void Update()
    {
        if (isWiping)
        {
            // Increase wipe progress
            wipeProgress += Time.deltaTime / wipeDuration;
            
            // Update all materials
            foreach (Renderer rend in renderers)
            {
                rend.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat("_WipeTheshold", wipeProgress);
                rend.SetPropertyBlock(propertyBlock);
            }
            
            // Stop when complete
            if (wipeProgress >= 2f)
            {
                isWiping = false;
                wipeProgress = 2f;
            }
        }
    }
    
    public void StartWipe()
    {
        wipeProgress = -.5f;
        isWiping = true;
    }
}