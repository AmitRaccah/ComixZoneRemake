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
    [SerializeField] private float preTrackerMovementDelay = 0.3f;

    [Header("Animation")]
    [SerializeField] private string animationStateName = "Stage_Pass";

    [Header("Tracker Manual Control (optional)")]
    [SerializeField] private TrackerFollowDeltaX trackerControl;

    [Header("Tracker Follow Settings")]
    [SerializeField] private float trackerMoveSpeed = 3f;

    [Header("Tracker End Point")]
    [SerializeField] private Transform trackerEndPoint;
    [SerializeField] private float trackerEndMoveSpeed = 2f;

    [Header("Down-Pass Settings")]
    [SerializeField] private bool requireCrouch = false;

    [Header("Camera Switch")]
    [SerializeField] private GameObject currentCamera;
    [SerializeField] private GameObject nextCamera;

    private bool triggered = false;
    private bool isHopping = false;
    private Vector3 trackerTarget;
    private Coroutine crouchWaitRoutine = null;
    private bool camerasSwitched = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        var input = other.GetComponent<StarterAssetsInputs>();
        if (input == null) return;

        if (requireCrouch)
        {
            if (input.crouch) BeginHop(other.transform, input);
            else
            {
                if (crouchWaitRoutine != null) StopCoroutine(crouchWaitRoutine);
                crouchWaitRoutine = StartCoroutine(WaitForCrouch(other.transform, input));
            }
        }
        else BeginHop(other.transform, input);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (crouchWaitRoutine != null) { StopCoroutine(crouchWaitRoutine); crouchWaitRoutine = null; }
    }

    IEnumerator WaitForCrouch(Transform player, StarterAssetsInputs input)
    {
        while (!triggered)
        {
            if (player == null || input == null) yield break;
            if (input.crouch) { BeginHop(player, input); yield break; }
            yield return null;
        }
    }

    void BeginHop(Transform player, StarterAssetsInputs input)
    {
        triggered = true;
        camerasSwitched = false;

        if (crouchWaitRoutine != null) { StopCoroutine(crouchWaitRoutine); crouchWaitRoutine = null; }

        if (requireCrouch)
        {
            CoreBus.Publish(new PlayerUncrouchEvent());
            input.crouch = false;
        }

        StartCoroutine(HopRoutine(player));
    }

    void Update()
    {
        if (!isHopping || tracker == null) return;

        tracker.position = Vector3.MoveTowards(
            tracker.position,
            trackerTarget,
            trackerMoveSpeed * Time.deltaTime
        );
    }

    IEnumerator HopRoutine(Transform player)
    {
        if (player == null || tracker == null || worldLanding == null)
        {
            triggered = false;
            yield break;
        }

        var ctrl = player.GetComponent<ThirdPersonController>();
        var inp = player.GetComponent<StarterAssetsInputs>();
        var mLck = player.GetComponent<MovementLock>();
        var anim = player.GetComponentInChildren<Animator>();

        // נטרל שליטה + כבה follower
        if (ctrl) { ctrl.allowZMovementTemporarily = true; ctrl.enabled = false; }
        if (inp) inp.enabled = false;
        if (mLck) mLck.SetExternalLock(true);
        if (trackerControl) trackerControl.enabled = false;

        // אנימציה (מיידי)
        if (anim)
        {
            anim.SetBool("IsCrouching", false);
            anim.Play(animationStateName, 0, 0f);
        }

        // המתנה לפני טלפורט ראשון
        yield return new WaitForSeconds(preTeleportDelay);

        float yRot = player.eulerAngles.y;

        // הצמד לשורת ה-Tracker בתחילת המעבר
        Warp(player, tracker.position, yRot);

        // החלפת מצלמה
        if (!camerasSwitched)
        {
            yield return SwitchToNextCamera();
            camerasSwitched = true;
        }

        // הזזת ה-Tracker ל-TrackerTargetEmpty (אם קיים)
        if (trackerTargetEmpty != null)
        {
            trackerTarget = trackerTargetEmpty.position;

            if (preTrackerMovementDelay > 0f)
                yield return new WaitForSeconds(preTrackerMovementDelay);

            isHopping = true;
            while (tracker.position != trackerTarget)
            {
                Warp(player, tracker.position, yRot);
                yield return null;
            }
        }

        // טלפורט נחיתה סופי
        Warp(player, worldLanding.position, yRot);

        // סיום מצב Hop (עדיין לא מחזירים שליטה!)
        isHopping = false;

        // ה-Tracker ממשיך ל-EndPoint, תוך שמירה על פוזה/איידל במקום הליכה
        if (trackerEndPoint != null)
            yield return StartCoroutine(MoveTrackerToEndPointHoldIdle(player, worldLanding.position, yRot, anim));

        // מדליקים Follower ומסנכרנים
        if (trackerControl)
        {
            trackerControl.enabled = true;
            trackerControl.ResetSync();
        }

        // ורק עכשיו מחזירים שליטה
        if (ctrl) { ctrl.enabled = true; ctrl.allowZMovementTemporarily = false; }
        if (inp) inp.enabled = true;
        if (mLck) mLck.SetExternalLock(false);

        triggered = false;
    }

    // הגלשת ה-Tracker ל-EndPoint בלי לקפוא את האנימטור:
    // מבטלים זמנית RootMotion, מאפסים פרמטרי לוקומושן, ואם יש Idle - עושים CrossFade אליו.
    IEnumerator MoveTrackerToEndPointHoldIdle(Transform player, Vector3 landingPos, float yRot, Animator anim)
    {
        if (tracker == null || trackerEndPoint == null) yield break;

        bool prevApplyRoot = false;
        if (anim)
        {
            prevApplyRoot = anim.applyRootMotion;
            anim.applyRootMotion = false; // שלא יזיז את האבטר בפועל

            // איפוס פרמטרים שכיחים של לוקומושן (אם קיימים)
            TrySetFloat(anim, "Speed", 0f);
            TrySetFloat(anim, "MotionSpeed", 0f);
            TrySetBool(anim, "IsMoving", false);

            // מעבר עדין ל-Idle אם יש מצב כזה
            int idleHash = Animator.StringToHash("Idle");
            if (anim.HasState(0, idleHash)) anim.CrossFade(idleHash, 0.1f, 0, 0f);
        }

        Vector3 endTarget = trackerEndPoint.position;
        while (tracker.position != endTarget)
        {
            tracker.position = Vector3.MoveTowards(
                tracker.position,
                endTarget,
                trackerEndMoveSpeed * Time.deltaTime
            );

            // שמור את השחקן “תפור” ללנדינג
            Warp(player, landingPos, yRot);

            yield return null;
        }

        if (anim) anim.applyRootMotion = prevApplyRoot; // החזרה למצב המקורי
    }

    IEnumerator SwitchToNextCamera()
    {
        if (nextCamera != null) nextCamera.SetActive(true);
        yield return null;
        if (currentCamera != null) currentCamera.SetActive(false);
    }

    static void Warp(Transform t, Vector3 pos, float yRot)
    {
        if (!t) return;
        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
    }

    // עוזר קטן כדי לא להציף וורנינגים אם הפרמטר לא קיים
    static void TrySetFloat(Animator a, string name, float v)
    {
        if (!a) return;
        foreach (var p in a.parameters)
            if (p.type == AnimatorControllerParameterType.Float && p.name == name)
            { a.SetFloat(name, v); return; }
    }
    static void TrySetBool(Animator a, string name, bool v)
    {
        if (!a) return;
        foreach (var p in a.parameters)
            if (p.type == AnimatorControllerParameterType.Bool && p.name == name)
            { a.SetBool(name, v); return; }
    }
}
