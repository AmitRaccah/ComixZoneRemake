using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class DiegeticUIManager : MonoBehaviour
{
    [Header("Button Objects")]
    [SerializeField] private GameObject newGameButton;
    [SerializeField] private GameObject resumeButton;
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject exitButton;
    
    [Header("Settings Panel")]
    [SerializeField] private GameObject settingsPanel; // Can now be a Canvas
    [SerializeField] private bool hideButtonsWhenSettingsOpen = true;
    [SerializeField] private bool disableMenuInteractionWhenSettingsOpen = true;
    
    [Header("New Game Settings")]
    [SerializeField] private Animation newGameAnimation;
    [SerializeField] private string animationName; // Leave empty to use default animation
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float delayBeforeLoad = 1f;
    
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    
    [Header("Cursor Settings")]
    [SerializeField] private bool manageCursor = true;
    [SerializeField] private Texture2D customCursorTexture;
    [SerializeField] private Vector2 cursorHotspot = Vector2.zero;
    
    [Header("Hover Effect")]
    [SerializeField] private bool enableHoverEffect = true;
    [SerializeField] private float brightnessMultiplier = 1.3f; // How much brighter (1.3 = 30% brighter)
    
    private bool actionInProgress = false;
    private GameObject currentlyHovered = null;
    private Material[] originalMaterials;
    private Renderer[] buttonRenderers;

    void Start()
    {
        // Setup camera
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // Setup cursor
        if (manageCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            if (customCursorTexture != null)
            {
                Cursor.SetCursor(customCursorTexture, cursorHotspot, CursorMode.Auto);
            }
        }
        
        // Check for colliders on buttons
        CheckButtonColliders();
        
        // Hide settings panel at start
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        
        // Store original materials for hover effect
        if (enableHoverEffect)
        {
            StoreOriginalMaterials();
        }
    }
    
    void StoreOriginalMaterials()
    {
        GameObject[] buttons = { newGameButton, resumeButton, settingsButton, exitButton };
        buttonRenderers = new Renderer[buttons.Length];
        originalMaterials = new Material[buttons.Length];
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != null)
            {
                buttonRenderers[i] = buttons[i].GetComponent<Renderer>();
                if (buttonRenderers[i] != null)
                {
                    // Store a copy of the original material
                    originalMaterials[i] = new Material(buttonRenderers[i].material);
                }
            }
        }
    }

    void CheckButtonColliders()
    {
        if (newGameButton != null && newGameButton.GetComponent<Collider>() == null)
            Debug.LogWarning($"{newGameButton.name} needs a Collider for click detection!");
        
        if (resumeButton != null && resumeButton.GetComponent<Collider>() == null)
            Debug.LogWarning($"{resumeButton.name} needs a Collider for click detection!");
        
        if (settingsButton != null && settingsButton.GetComponent<Collider>() == null)
            Debug.LogWarning($"{settingsButton.name} needs a Collider for click detection!");
        
        if (exitButton != null && exitButton.GetComponent<Collider>() == null)
            Debug.LogWarning($"{exitButton.name} needs a Collider for click detection!");
    }

    void Update()
    {
        // Don't interact with menu buttons if settings is open and interaction is disabled
        bool canInteractWithMenu = !actionInProgress && 
                                   !(settingsPanel != null && settingsPanel.activeSelf && disableMenuInteractionWhenSettingsOpen);
        
        if (canInteractWithMenu)
        {
            // Check for hover effect
            if (enableHoverEffect)
            {
                CheckHover();
            }
            
            // Check for click
            if (Input.GetMouseButtonDown(0))
            {
                CheckForButtonClick();
            }
        }
    }
    
    void CheckHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        GameObject hoveredObject = null;
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;
            
            // Check if it's one of our buttons
            if (hitObject == newGameButton || hitObject == resumeButton || 
                hitObject == settingsButton || hitObject == exitButton)
            {
                hoveredObject = hitObject;
            }
        }
        
        // If we're hovering over a different object than before
        if (hoveredObject != currentlyHovered)
        {
            // Reset the previously hovered object
            if (currentlyHovered != null)
            {
                ResetMaterial(currentlyHovered);
            }
            
            // Brighten the newly hovered object
            if (hoveredObject != null)
            {
                BrightenMaterial(hoveredObject);
            }
            
            currentlyHovered = hoveredObject;
        }
    }
    
    void BrightenMaterial(GameObject obj)
    {
        GameObject[] buttons = { newGameButton, resumeButton, settingsButton, exitButton };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == obj && buttonRenderers[i] != null)
            {
                Color originalColor = originalMaterials[i].color;
                Color brightColor = originalColor * brightnessMultiplier;
                brightColor.a = originalColor.a; // Keep original alpha
                buttonRenderers[i].material.color = brightColor;
                break;
            }
        }
    }
    
    void ResetMaterial(GameObject obj)
    {
        GameObject[] buttons = { newGameButton, resumeButton, settingsButton, exitButton };
        
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == obj && buttonRenderers[i] != null)
            {
                buttonRenderers[i].material.color = originalMaterials[i].color;
                break;
            }
        }
    }

    void CheckForButtonClick()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            GameObject clickedObject = hit.collider.gameObject;
            
            // Check which button was clicked
            if (clickedObject == newGameButton)
            {
                OnNewGameClicked();
            }
            else if (clickedObject == resumeButton)
            {
                OnResumeClicked();
            }
            else if (clickedObject == settingsButton)
            {
                OnSettingsClicked();
            }
            else if (clickedObject == exitButton)
            {
                OnExitClicked();
            }
        }
    }

    void OnNewGameClicked()
    {
        actionInProgress = true;
        
        // Reset hover effect
        if (currentlyHovered != null)
        {
            ResetMaterial(currentlyHovered);
            currentlyHovered = null;
        }
        
        Debug.Log("New Game clicked!");
        
        if (newGameAnimation != null)
        {
            if (!string.IsNullOrEmpty(animationName))
            {
                newGameAnimation.Play(animationName);
            }
            else
            {
                newGameAnimation.Play();
            }
            
            StartCoroutine(WaitForAnimationAndLoadScene());
        }
        else
        {
            // If no animation, just load scene directly
            LoadScene(sceneToLoad);
        }
    }
    
    void OnDestroy()
    {
        // Clean up material copies to avoid memory leaks
        if (originalMaterials != null)
        {
            foreach (Material mat in originalMaterials)
            {
                if (mat != null)
                {
                    Destroy(mat);
                }
            }
        }
    }

    void OnResumeClicked()
    {
        Debug.Log("Resume clicked!");
        
        // Add your resume logic here
        // For example: Time.timeScale = 1; or load saved game state
        
        // Example: Hide cursor and unpause
        if (manageCursor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        // You might want to disable the menu UI here
        gameObject.SetActive(false);
    }

    void OnSettingsClicked()
    {
        Debug.Log("Settings clicked!");
        ToggleSettings();
    }
    
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            // Toggle settings panel
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
            
            // Optionally hide menu buttons when settings is open
            if (hideButtonsWhenSettingsOpen)
            {
                if (newGameButton != null) newGameButton.SetActive(isActive);
                if (resumeButton != null) resumeButton.SetActive(isActive);
                if (settingsButton != null) settingsButton.SetActive(isActive);
                if (exitButton != null) exitButton.SetActive(isActive);
            }
        }
    }
    
    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            ToggleSettings();
        }
    }

    void OnExitClicked()
    {
        Debug.Log("Exit clicked!");
        
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    IEnumerator WaitForAnimationAndLoadScene()
    {
        string clipName = string.IsNullOrEmpty(animationName) 
            ? newGameAnimation.clip.name 
            : animationName;
        
        // Wait until animation starts
        while (!newGameAnimation.IsPlaying(clipName))
        {
            yield return null;
        }
        
        // Wait until animation finishes
        while (newGameAnimation.IsPlaying(clipName))
        {
            yield return null;
        }
        
        // Wait for delay
        yield return new WaitForSeconds(delayBeforeLoad);
        
        // Load scene
        LoadScene(sceneToLoad);
    }

    void LoadScene(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            // Hide cursor before loading
            if (manageCursor)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
            
            Debug.Log($"Loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene name is not specified!");
        }
    }
}