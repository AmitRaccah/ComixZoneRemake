using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlackScreenTextManager : MonoBehaviour
{
    [Header("Text Settings")]
    [TextArea(3, 10)]
    public string transitionText = "Loading...";
    public bool showTextOnTransition = true;
    public Color textColor = Color.white;
    public int fontSize = 48;
    public Font customFont;
    
    [Header("Text Timing")]
    public float textDelayAfterBlackScreen = 0.5f;
    public float textFadeInDuration = 1f;
    public float textDisplayDuration = 2f;
    public float textFadeOutDuration = 1f;
    
    [Header("Text Position")]
    public TextAnchor textAlignment = TextAnchor.MiddleCenter;
    public Vector2 textOffset = Vector2.zero;
    
    private Canvas textCanvas;
    private Text textComponent;
    private CanvasGroup textCanvasGroup;
    private Coroutine textDisplayCoroutine;
    
    public static BlackScreenTextManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CreateTextCanvas();
    }
    
    void CreateTextCanvas()
    {
        GameObject canvasObject = new GameObject("BlackScreenTextCanvas");
        canvasObject.transform.SetParent(transform);
        
        textCanvas = canvasObject.AddComponent<Canvas>();
        textCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        textCanvas.sortingOrder = 10000;
        
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        textCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        textCanvasGroup.alpha = 0f;
        textCanvasGroup.interactable = false;
        textCanvasGroup.blocksRaycasts = false;
        
        GameObject textObject = new GameObject("TransitionText");
        textObject.transform.SetParent(canvasObject.transform);
        
        textComponent = textObject.AddComponent<Text>();
        textComponent.text = transitionText;
        textComponent.color = textColor;
        textComponent.fontSize = fontSize;
        textComponent.alignment = textAlignment;
        textComponent.raycastTarget = false;
        
        if (customFont != null)
        {
            textComponent.font = customFont;
        }
        else
        {
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.anchoredPosition = textOffset;
        
        canvasObject.SetActive(false);
    }
    
    public void ShowTransitionText()
    {
        ShowTransitionText(transitionText);
    }
    
    public void ShowTransitionText(string text)
    {
        ShowTransitionText(text, textDelayAfterBlackScreen);
    }
    
    public void ShowTransitionText(string text, float customDelay)
    {
        if (!showTextOnTransition || textComponent == null)
        {
            return;
        }
        
        if (textDisplayCoroutine != null)
        {
            StopCoroutine(textDisplayCoroutine);
        }
        
        textComponent.text = text;
        
        textDisplayCoroutine = StartCoroutine(DisplayTextSequence(customDelay));
    }
    
    public void ShowTextImmediate(string text)
    {
        if (textComponent == null)
        {
            return;
        }
        
        textComponent.text = text;
        textCanvas.gameObject.SetActive(true);
        textCanvasGroup.alpha = 1f;
    }
    
    public void HideTextImmediate()
    {
        if (textCanvas != null)
        {
            textCanvas.gameObject.SetActive(false);
            textCanvasGroup.alpha = 0f;
        }
    }
    
    public void SetTransitionText(string text)
    {
        transitionText = text;
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
    
    IEnumerator DisplayTextSequence()
    {
        return DisplayTextSequence(textDelayAfterBlackScreen);
    }
    
    IEnumerator DisplayTextSequence(float customDelay)
    {
        if (customDelay > 0)
        {
            yield return new WaitForSeconds(customDelay);
        }
        
        textCanvas.gameObject.SetActive(true);
        
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 0f, 1f, textFadeInDuration));
        
        if (textDisplayDuration > 0)
        {
            yield return new WaitForSeconds(textDisplayDuration);
        }
        
        yield return StartCoroutine(FadeCanvasGroup(textCanvasGroup, 1f, 0f, textFadeOutDuration));
        
        textCanvas.gameObject.SetActive(false);
    }
    
    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        if (duration <= 0)
        {
            group.alpha = endAlpha;
            yield break;
        }
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        
        group.alpha = endAlpha;
    }
    
    public void SetTextColor(Color color)
    {
        textColor = color;
        if (textComponent != null)
        {
            textComponent.color = color;
        }
    }
    
    public void SetFontSize(int size)
    {
        fontSize = size;
        if (textComponent != null)
        {
            textComponent.fontSize = size;
        }
    }
    
    public void SetTextAlignment(TextAnchor alignment)
    {
        textAlignment = alignment;
        if (textComponent != null)
        {
            textComponent.alignment = alignment;
        }
    }
    
    public void SetShowTextOnTransition(bool show)
    {
        showTextOnTransition = show;
    }
    
    public string GetTransitionText()
    {
        return transitionText;
    }
}