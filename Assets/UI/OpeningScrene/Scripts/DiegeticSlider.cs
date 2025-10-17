using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class DiegeticSlider : MonoBehaviour
{
    [Header("Slider Settings")]
    [SerializeField] private Transform sliderHandle; // The knob/handle that moves
    [SerializeField] private Transform sliderTrack; // The track it moves along
    [SerializeField] private Vector3 slideDirection = Vector3.right; // Direction of movement (usually right or up)
    [SerializeField] private float slideDistance = 1f; // How far it can slide (in units)
    
    [Header("Value Settings")]
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 1f;
    [SerializeField] private float startValue = 0.5f;
    
    [Header("Audio Settings (Optional)")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string mixerParameter = "MusicVolume"; // e.g., "MusicVolume" or "SFXVolume"
    
    [Header("Camera")]
    [SerializeField] private Camera playerCamera;
    
    [Header("Events")]
    public UnityEvent<float> onValueChanged; // For custom functionality
    
    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource; // Optional audio source for slider sounds
    [SerializeField] private AudioClip dragSound; // Sound while dragging
    [SerializeField] private AudioClip releaseSound; // Sound when releasing slider
    [SerializeField] private float soundVolume = 0.5f;
    
    private bool isDragging = false;
    private Vector3 startPosition;
    private float currentValue;
    private Plane dragPlane;
    private bool wasDraggingSoundPlaying = false;
    
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        
        // Store the handle's starting position relative to its parent (not the track)
        if (sliderHandle != null)
        {
            startPosition = sliderHandle.localPosition;
        }
        
        // Setup audio source if needed
        if (audioSource == null && (dragSound != null || releaseSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0; // 2D sound
            audioSource.loop = false;
        }
        
        // Set initial value AFTER storing start position
        SetValue(startValue);
    }
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag();
        }
        
        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                UpdateDrag();
            }
            else
            {
                StopDrag();
            }
        }
    }
    
    void TryStartDrag()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Check if we clicked on the handle or anything that's part of this slider
            Transform clickedTransform = hit.collider.transform;
            
            // Check if clicked object is the handle or a child of the handle
            bool clickedHandle = (clickedTransform == sliderHandle || clickedTransform.IsChildOf(sliderHandle));
            
            // Or if clicked on the track or this object
            bool clickedTrack = (clickedTransform == sliderTrack || clickedTransform.IsChildOf(sliderTrack) || clickedTransform == transform);
            
            if (clickedHandle || clickedTrack)
            {
                isDragging = true;
                
                // Play drag sound
                if (audioSource != null && dragSound != null)
                {
                    audioSource.clip = dragSound;
                    audioSource.loop = true;
                    audioSource.volume = soundVolume;
                    audioSource.Play();
                    wasDraggingSoundPlaying = true;
                }
                
                // Create a plane for dragging based on the slider's orientation
                Vector3 planeNormal = Vector3.Cross(slideDirection, playerCamera.transform.forward);
                if (planeNormal.magnitude < 0.01f)
                    planeNormal = Vector3.Cross(slideDirection, playerCamera.transform.up);
                
                dragPlane = new Plane(planeNormal.normalized, sliderHandle.position);
            }
        }
    }
    
    void UpdateDrag()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        float enter;
        
        if (dragPlane.Raycast(ray, out enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            
            // Get the world direction of sliding
            Vector3 worldSlideDirection = sliderHandle.parent.TransformDirection(slideDirection.normalized);
            
            // Get the world position where sliding starts (handle's starting world position)
            Vector3 handleStartWorld = sliderHandle.parent.TransformPoint(startPosition);
            
            // Project the hit point onto the slide direction
            float projectedDistance = Vector3.Dot(hitPoint - handleStartWorld, worldSlideDirection);
            
            // Clamp to slide distance
            projectedDistance = Mathf.Clamp(projectedDistance, 0f, slideDistance);
            
            // Update handle position
            sliderHandle.localPosition = startPosition + slideDirection.normalized * projectedDistance;
            
            // Calculate value (0 to 1)
            float normalizedValue = projectedDistance / slideDistance;
            currentValue = Mathf.Lerp(minValue, maxValue, normalizedValue);
            
            // Apply to audio mixer if set
            if (audioMixer != null && !string.IsNullOrEmpty(mixerParameter))
            {
                // Convert to decibels (Unity's audio mixer uses dB)
                float volumeDB = Mathf.Log10(Mathf.Max(currentValue, 0.0001f)) * 20f;
                audioMixer.SetFloat(mixerParameter, volumeDB);
            }
            
            // Trigger event
            onValueChanged?.Invoke(currentValue);
        }
    }
    
    void StopDrag()
    {
        isDragging = false;
        
        // Stop drag sound and play release sound
        if (audioSource != null)
        {
            if (wasDraggingSoundPlaying)
            {
                audioSource.Stop();
                wasDraggingSoundPlaying = false;
            }
            
            if (releaseSound != null)
            {
                audioSource.PlayOneShot(releaseSound, soundVolume);
            }
        }
    }
    
    public void SetValue(float value)
    {
        currentValue = Mathf.Clamp(value, minValue, maxValue);
        float normalizedValue = Mathf.InverseLerp(minValue, maxValue, currentValue);
        
        if (sliderHandle != null)
        {
            sliderHandle.localPosition = startPosition + slideDirection.normalized * (normalizedValue * slideDistance);
        }
        
        // Apply to audio mixer
        if (audioMixer != null && !string.IsNullOrEmpty(mixerParameter))
        {
            float volumeDB = Mathf.Log10(Mathf.Max(currentValue, 0.0001f)) * 20f;
            audioMixer.SetFloat(mixerParameter, volumeDB);
        }
    }
    
    public float GetValue()
    {
        return currentValue;
    }
}