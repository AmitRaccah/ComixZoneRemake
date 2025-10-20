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
    [SerializeField] private GameObject settingsPanel; // 3D diegetic settings panel
    [SerializeField] private bool hideButtonsWhenSettingsOpen = true;
    [SerializeField] private bool disableMenuInteractionWhenSettingsOpen = true;
    [SerializeField] private bool parentSettingsPanelToCamera = true;
    [SerializeField] private Vector3 settingsPanelLocalPosition = new Vector3(0, 0, 1.5f);
    [SerializeField] private Vector3 settingsPanelLocalRotation = new Vector3(0, 0, 0);
    
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
    
    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // Audio source for UI sounds
    [SerializeField] private AudioClip hoverSound; // Sound when hovering over buttons
    [SerializeField] private AudioClip clickSound; // Sound when clicking buttons
    [SerializeField] private AudioClip settingsOpenSound; // Sound when opening settings
    [SerializeField] private AudioClip settingsCloseSound; // Sound when closing settings
    [SerializeField] private float soundVolume = 1f;
    
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
        
        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0; // 2D sound
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
            
            // Parent to camera if option is enabled
            if (parentSettingsPanelToCamera && playerCamera != null)
            {
                settingsPanel.transform.SetParent(playerCamera.transform);
                settingsPanel.transform.localPosition = settingsPanelLocalPosition;
                settingsPanel.transform.localRotation = Quaternion.Euler(settingsPanelLocalRotation);
            }
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
        if (actionInProgress)
            return;
        
        // Check if settings is open
        bool settingsIsOpen = settingsPanel != null && settingsPanel.activeSelf;
        
        // Always check for clicks
        if (Input.GetMouseButtonDown(0))
        {
            CheckForButtonClick();
        }
        
        // Only check hover effect if we can interact with menu buttons
        if (enableHoverEffect && !(settingsIsOpen && disableMenuInteractionWhenSettingsOpen))
        {
            CheckHover();
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
                PlaySound(hoverSound); // Play hover sound
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
            // If settings panel is open and we clicked something that's not part of it, close it
            else if (settingsPanel != null && settingsPanel.activeSelf && !IsPartOfSettingsPanel(clickedObject))
            {
                CloseSettings();
            }
        }
        // Clicked on nothing and settings is open - close it
        else if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
        }
    }
    
    bool IsPartOfSettingsPanel(GameObject obj)
    {
        // Check if the clicked object is the settings panel or a child of it
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.gameObject == settingsPanel)
                return true;
            current = current.parent;
        }
        return false;
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
        
        PlaySound(clickSound); // Play click sound
        
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
        PlaySound(clickSound); // Play click sound
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
        PlaySound(clickSound); // Play click sound
        Debug.Log("Settings clicked!");
        ToggleSettings();
    }
    
    public void ToggleSettings()
    {
        if (settingsPanel != null)
        {
            // Toggle settings panel
            bool wasActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!wasActive);
            
            // Play appropriate sound
            if (!wasActive)
            {
                PlaySound(settingsOpenSound);
            }
            else
            {
                PlaySound(settingsCloseSound);
            }
            
            // Optionally hide menu buttons when settings is open
            if (hideButtonsWhenSettingsOpen)
            {
                if (newGameButton != null) newGameButton.SetActive(wasActive);
                if (resumeButton != null) resumeButton.SetActive(wasActive);
                if (settingsButton != null) settingsButton.SetActive(wasActive);
                if (exitButton != null) exitButton.SetActive(wasActive);
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
        PlaySound(clickSound); // Play click sound
        Debug.Log("Exit clicked!");
        
        // Quit the application
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, soundVolume);
        }
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