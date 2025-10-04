using UnityEngine;
using System.Collections;

public class CameraSwitchOnAnimationComplete : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animation animationComponent;
    [SerializeField] private string animationName; // Leave empty to use default animation
    
    [Header("Camera Settings")]
    [SerializeField] private Camera camera1;
    [SerializeField] private Camera camera2;
    [SerializeField] private float delayBeforeSwitch = 1f;
    
    private bool hasTriggered = false;

    void Start()
    {
        // Make sure only camera1 is active at start
        if (camera1 != null) camera1.enabled = true;
        if (camera2 != null) camera2.enabled = false;
        
        // Start checking for animation completion
        StartCoroutine(WaitForAnimationToComplete());
    }

    IEnumerator WaitForAnimationToComplete()
    {
        // Wait until the animation component exists
        while (animationComponent == null)
        {
            yield return null;
        }
        
        // If no animation name specified, use the default clip
        string clipName = string.IsNullOrEmpty(animationName) 
            ? animationComponent.clip.name 
            : animationName;
        
        // Wait until animation starts playing
        while (!animationComponent.IsPlaying(clipName))
        {
            yield return null;
        }
        
        // Wait until animation finishes
        while (animationComponent.IsPlaying(clipName))
        {
            yield return null;
        }
        
        // Animation finished, wait for the delay
        yield return new WaitForSeconds(delayBeforeSwitch);
        
        // Switch cameras
        if (!hasTriggered)
        {
            SwitchCamera();
            hasTriggered = true;
        }
    }

    void SwitchCamera()
    {
        if (camera1 != null && camera2 != null)
        {
            camera1.enabled = false;
            camera2.enabled = true;
            Debug.Log("Switched from Camera 1 to Camera 2");
        }
    }
}