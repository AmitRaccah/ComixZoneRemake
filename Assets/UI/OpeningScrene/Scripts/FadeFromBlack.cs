using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Add this script to an empty GameObject in your NEW scene to fade FROM black when the scene starts.
/// This completes the transition from the menu scene.
/// </summary>
public class FadeFromBlack : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float delayBeforeFade = 0.5f; // Optional delay before fading in
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private bool destroyFadeCanvasFromPreviousScene = true;
    
    private Canvas fadeCanvas;
    private Image fadeImage;

    void Start()
    {
        // Destroy the fade canvas from the previous scene if it exists
        if (destroyFadeCanvasFromPreviousScene)
        {
            GameObject oldFadeCanvas = GameObject.Find("FadeCanvas");
            if (oldFadeCanvas != null)
            {
                Destroy(oldFadeCanvas);
            }
        }
        
        // Create our own fade canvas for this scene
        CreateFadeCanvas();
        
        // Start the fade-in effect
        StartCoroutine(FadeIn());
    }
    
    void CreateFadeCanvas()
    {
        // Create canvas
        GameObject canvasObject = new GameObject("FadeCanvas");
        fadeCanvas = canvasObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 9999;
        
        // Add Canvas Scaler
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add Graphic Raycaster
        canvasObject.AddComponent<GraphicRaycaster>();
        
        // Create image
        GameObject imageObject = new GameObject("FadeImage");
        imageObject.transform.SetParent(canvasObject.transform);
        
        fadeImage = imageObject.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f); // Start fully opaque (black)
        
        // Stretch to fill entire screen
        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = Vector2.zero;
    }
    
    IEnumerator FadeIn()
    {
        // Optional delay while screen is black
        if (delayBeforeFade > 0)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }
        
        // Fade from black to transparent
        float elapsed = 0f;
        Color startColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);
        Color endColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            fadeImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }
        
        // Ensure fully transparent
        fadeImage.color = endColor;
        
        // Destroy the fade canvas after fade completes
        Destroy(fadeCanvas.gameObject);
    }
}