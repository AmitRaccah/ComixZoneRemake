using UnityEngine;

public class CanvasFollowCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float distanceFromCamera = 2f;
    [SerializeField] private Vector3 offset = Vector3.zero; // Additional offset if needed
    
    [Header("Smoothing (Optional)")]
    [SerializeField] private bool useSmoothing = false;
    [SerializeField] private float smoothSpeed = 5f;
    
    private Canvas canvas;
    
    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
        }
    }
    
    void LateUpdate()
    {
        if (targetCamera == null) return;
        
        // Calculate target position
        Vector3 targetPosition = targetCamera.transform.position + 
                                 targetCamera.transform.forward * distanceFromCamera + 
                                 offset;
        
        // Calculate target rotation (face the camera)
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - targetCamera.transform.position);
        
        if (useSmoothing)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
}