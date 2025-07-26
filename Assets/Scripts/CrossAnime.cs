using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;    // Cinemachine 3.x

public class CrossAnime : MonoBehaviour
{
    /* ───── Animation ───── */
    [Header("Animation")]
    [SerializeField] private string animationTriggerName = "Pass";
    [SerializeField] private string animationStateName = "Stage_Pass";

    /* ───── Movement ───── */
    [Header("Movement")]
    [SerializeField] private Transform pathPoint;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float endDelay = 0.1f;

    /* ───── Camera ───── */
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera currentCam;
    [SerializeField] private CinemachineVirtualCamera targetCam;
    [SerializeField] private int camPriorityActive = 20;
    [SerializeField] private int camPriorityInactive = 5;

    /* ───── Colliders to disable during pass ───── */
    [Header("Colliders")]
    [SerializeField] private Collider[] collidersToDisable;   // wall, floor, etc.

    /* ───── Down‑Pass Settings ───── */
    [Header("Down Pass")]
    [Tooltip("Enable only on DOWN‑colliders. Leave off for forward colliders.")]
    [SerializeField] private bool requireCrouch = false;

    /* ───────── State ───────── */
    bool _triggered;
    bool _playerInside;
    Coroutine _waitRoutine;

    /* ───────── Trigger Entry ───────── */
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _triggered) return;
        _playerInside = true;

        var input = other.GetComponent<StarterAssets.StarterAssetsInputs>();
        if (input == null) return;

        /* Forward collider – start immediately */
        if (!requireCrouch)
        {
            BeginPass(other.transform, input);
            return;
        }

        /* Down collider – start if already crouching, else wait */
        if (input.crouch)
            BeginPass(other.transform, input);
        else
            _waitRoutine = StartCoroutine(WaitForCrouch(input, other.transform));
    }

    /* ───────── Trigger Exit ───────── */
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInside = false;

        /* Cancel waiting coroutine if player left before crouching */
        if (!_triggered && _waitRoutine != null)
        {
            StopCoroutine(_waitRoutine);
            _waitRoutine = null;
        }
    }

    /* ───────── Wait until crouch is pressed ───────── */
    IEnumerator WaitForCrouch(StarterAssets.StarterAssetsInputs input, Transform player)
    {
        while (!_triggered && _playerInside)
        {
            if (input.crouch)
            {
                BeginPass(player, input);
                yield break;
            }
            yield return null;
        }
    }

    /* ───────── Main Entry Point ───────── */
    void BeginPass(Transform player, StarterAssets.StarterAssetsInputs input)
    {
        _triggered = true;
        /* reset crouch immediately so animation isn’t blocked */
        if (requireCrouch && input.crouch)
        {
            CoreBus.Publish(new PlayerUncrouchEvent());
            input.crouch = false;
        }
        StartCoroutine(DoStagePass(player));
    }

    /* ───────── Main Coroutine ───────── */
    IEnumerator DoStagePass(Transform player)
    {
        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
        var input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
        var movementLock = player.GetComponent<MovementLock>();

        /* Disable control */
        if (controller)
        {
            controller.allowZMovementTemporarily = true;
            controller.enabled = false;
        }
        if (input) input.enabled = false;
        if (movementLock) movementLock.SetExternalLock(true);

        /* Disable colliders (wall, floor, etc.) */
        foreach (var col in collidersToDisable)
            if (col) col.enabled = false;

        /* Play animation */
        var anim = player.GetComponentInChildren<Animator>();
        if (anim)
        {
            anim.SetBool("IsCrouching", false);
            anim.SetTrigger(animationTriggerName);
        }

        yield return new WaitUntil(() =>
            anim == null ||
            (!anim.IsInTransition(0) &&
             anim.GetCurrentAnimatorStateInfo(0).IsName(animationStateName)));

        /* Switch cameras */
        if (targetCam)
        {
            if (currentCam) currentCam.Priority = camPriorityInactive;
            targetCam.Priority = camPriorityActive;
            currentCam = targetCam;
            targetCam = null;
        }

        /* Tween movement */
        if (pathPoint)
        {
            float distance = Vector3.Distance(player.position, pathPoint.position);
            float duration = distance / moveSpeed;

            yield return player.DOMove(pathPoint.position, duration)
                               .SetEase(Ease.Linear)
                               .WaitForCompletion();
        }

        yield return new WaitForSeconds(endDelay);

        /* Re‑enable control */
        if (controller)
        {
            controller.enabled = true;
            controller.allowZMovementTemporarily = false;
        }
        if (input) input.enabled = true;
        if (movementLock) movementLock.SetExternalLock(false);

        /* Re‑enable colliders */
        foreach (var col in collidersToDisable)
            if (col) col.enabled = true;

        Destroy(this);
    }
}
