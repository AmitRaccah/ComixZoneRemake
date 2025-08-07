using UnityEngine;
using System.Collections;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHop : MonoBehaviour
{
    [SerializeField] private Transform tracker;
    [SerializeField] private Transform worldLanding;
    [SerializeField] private Transform trackerTargetEmpty;

    [Header("Timings")]
    [SerializeField] private float preTeleportDelay = 0.4f;

    [Header("Animation")]
    [SerializeField] private string animationStateName = "Stage_Pass";
    [SerializeField] private float crossFadeTime = 0.05f;

    [Header("Tracker Manual Control (optional)")]
    [SerializeField] private TrackerManualControl trackerControl;

    [SerializeField] private Collider[] collidersToDisable;

    [Header("Tracker Follow Settings")]
    [SerializeField] private float trackerMoveSpeed = 3f;
    [SerializeField] private float trackerArrivalThreshold = 0.01f;

    [Header("Down-Pass Settings")]
    [SerializeField] private bool requireCrouch = false;

    private bool triggered = false;
    private bool isHopping = false;
    private Vector3 trackerTarget;
    private Coroutine crouchWaitRoutine = null;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        var input = other.GetComponent<StarterAssetsInputs>();
        if (input == null) return;

        if (requireCrouch)
        {
            if (input.crouch)
            {
                BeginHop(other.transform, input);
            }
            else
            {
                crouchWaitRoutine = StartCoroutine(WaitForCrouch(other.transform, input));
            }
        }
        else
        {
            BeginHop(other.transform, input);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (crouchWaitRoutine != null)
        {
            StopCoroutine(crouchWaitRoutine);
            crouchWaitRoutine = null;
        }
    }

    IEnumerator WaitForCrouch(Transform player, StarterAssetsInputs input)
    {
        while (!triggered)
        {
            if (input == null || player == null) yield break;

            if (input.crouch)
            {
                BeginHop(player, input);
                yield break;
            }

            yield return null;
        }
    }

    void BeginHop(Transform player, StarterAssetsInputs input)
    {
        triggered = true;

        if (requireCrouch)
        {
            CoreBus.Publish(new PlayerUncrouchEvent());
            input.crouch = false;
        }

        StartCoroutine(HopRoutine(player));
    }

    void Update()
    {
        if (isHopping)
        {
            tracker.position = Vector3.MoveTowards(
                tracker.position,
                trackerTarget,
                trackerMoveSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator HopRoutine(Transform player)
    {
        var ctrl = player.GetComponent<ThirdPersonController>();
        var inp = player.GetComponent<StarterAssetsInputs>();
        var mLck = player.GetComponent<MovementLock>();

        if (ctrl) { ctrl.allowZMovementTemporarily = true; ctrl.enabled = false; }
        if (inp) inp.enabled = false;
        if (mLck) mLck.SetExternalLock(true);
        foreach (var c in collidersToDisable) if (c) c.enabled = false;
        if (trackerControl) trackerControl.enabled = false;

        var anim = player.GetComponentInChildren<Animator>();
        if (anim)
        {
            anim.SetBool("IsCrouching", false); // safety
            anim.CrossFadeInFixedTime(animationStateName, crossFadeTime, 0, 0f);
        }

        yield return new WaitForSeconds(Mathf.Max(preTeleportDelay, crossFadeTime));

        float yRot = player.eulerAngles.y;
        Warp(player, tracker.position, yRot);

        if (trackerTargetEmpty != null)
        {
            trackerTarget = trackerTargetEmpty.position;
            isHopping = true;

            while (Vector3.Distance(tracker.position, trackerTarget) > trackerArrivalThreshold)
            {
                Warp(player, tracker.position, yRot);
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("[PanelHop] trackerTargetEmpty לא הוגדר!");
        }

        Warp(player, worldLanding.position, yRot);

        isHopping = false;
        if (ctrl) { ctrl.enabled = true; ctrl.allowZMovementTemporarily = false; }
        if (inp) inp.enabled = true;
        if (mLck) mLck.SetExternalLock(false);
        foreach (var c in collidersToDisable) if (c) c.enabled = true;
        if (trackerControl) trackerControl.enabled = true;

        triggered = false;
    }

    static void Warp(Transform t, Vector3 pos, float yRot)
    {
        if (!t) return;
        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
