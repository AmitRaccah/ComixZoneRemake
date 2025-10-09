using UnityEngine;

public class MovementBasicsObjective : TutorialObjective
{
    [Header("Balloon")]
    [SerializeField] private Sprite promptSprite;

    [Header("References")]
    [SerializeField] private Transform trackedTransform;
    [SerializeField] private Animator animator;

    [Header("Requirements")]
    [SerializeField] private float requiredSideDistance = 0.5f;
    [SerializeField] private string lookUpBool = "IsLookUp";
    [SerializeField] private string crouchBool = "IsCrouching";

    float startX;
    bool movedRight, movedLeft, lookedUp, crouched;
    int lookUpHash, crouchHash;

    protected override void OnBegin()
    {
        if (promptSprite) Manager.ShowBalloon(promptSprite);

        if (!trackedTransform && animator) trackedTransform = animator.transform;
        if (!animator && trackedTransform) animator = trackedTransform.GetComponent<Animator>();

        startX = trackedTransform ? trackedTransform.position.x : 0f;
        movedRight = movedLeft = lookedUp = crouched = false;

        lookUpHash = Animator.StringToHash(lookUpBool);
        crouchHash = Animator.StringToHash(crouchBool);
    }

    protected override void OnEnd()
    {
        Manager.HideBalloon();
    }

    protected override void OnComplete()
    {
        Manager.HideBalloon();
    }

    void Update()
    {
        if (!IsActive) return;

        if (trackedTransform)
        {
            float x = trackedTransform.position.x;
            if (!movedRight && x >= startX + requiredSideDistance) movedRight = true;
            if (!movedLeft && x <= startX - requiredSideDistance) movedLeft = true;
        }

        if (animator)
        {
            if (!lookedUp && animator.GetBool(lookUpHash)) lookedUp = true;
            if (!crouched && animator.GetBool(crouchHash)) crouched = true;
        }

        if (movedRight && movedLeft && lookedUp && crouched)
            CompleteObjective();
    }
}
