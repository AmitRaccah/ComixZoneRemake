using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LoadSceneOnAnimationComplete : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animation animationComponent;
    [SerializeField] private string animationName; // Leave empty to use default animation
    
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad; // Name of the scene to load
    [SerializeField] private float delayBeforeLoad = 1f;
    
    private bool hasTriggered = false;

    void Start()
    {
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
        yield return new WaitForSeconds(delayBeforeLoad);
        
        // Load the new scene
        if (!hasTriggered)
        {
            LoadScene();
            hasTriggered = true;
        }
    }

    void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.Log($"Loading scene: {sceneToLoad}");
            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("Scene name is not specified!");
        }
    }
}