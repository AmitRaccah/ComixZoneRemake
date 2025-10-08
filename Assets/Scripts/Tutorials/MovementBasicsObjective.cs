using UnityEngine;

public class MovementBasicsObjective : TutorialObjective
{
    [Header("References")]
    [SerializeField] private Transform trackedTransform; 
    [SerializeField] private Animator animator;         

    [Header("Distance to move each side (meters)")]
    [SerializeField] private float requiredSideDistance = 0.5f; 

    [Header("Animator bool names")]
    [SerializeField] private string lookUpBool = "IsLookUp";   
    [SerializeField] private string crouchBool = "IsCrouching";   

    private float lastX;
    private float movedRight; 
    private float movedLeft; 
    private bool didRight, didLeft, didUp, didDown;

    protected override void OnConfigure(TutorialManager manager)
    {
        if (!trackedTransform)
        {
            Debug.LogError("[MovementBasicsObjective] trackedTransform is missing (player Transform).");
        }
        if (!animator && trackedTransform)
        {
            animator = trackedTransform.GetComponentInChildren<Animator>();
            if (!animator)
                Debug.LogError("[MovementBasicsObjective] Animator is missing.");
        }
    }

    protected override void OnReset()
    {
        movedRight = movedLeft = 0f;
        didRight = didLeft = didUp = didDown = false;
        if (trackedTransform) lastX = trackedTransform.position.x;
    }

    void Update()
    {
        if (!IsActive || trackedTransform == null || animator == null)
            return;

        float x = trackedTransform.position.x;
        float dx = x - lastX;
        lastX = x;

        if (dx > 0f) movedRight += dx;
        else if (dx < 0f) movedLeft += -dx;

        if (!didRight && movedRight >= requiredSideDistance) didRight = true;
        if (!didLeft && movedLeft >= requiredSideDistance) didLeft = true;

        if (!didUp && animator.GetBool(lookUpBool)) didUp = true;
        if (!didDown && animator.GetBool(crouchBool)) didDown = true;

        if (didRight && didLeft && didUp && didDown)
            CompleteObjective();
    }
}
