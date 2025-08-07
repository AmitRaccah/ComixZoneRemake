using UnityEngine;
using System.Collections;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHop : MonoBehaviour
{
    [Header("Key Transforms")]
    [Tooltip("האובייקט שמייצג את הספירה על הלוח")]
    [SerializeField] private Transform tracker;

    [Tooltip("נקודת הנחיתה בעולם - אחרי הלוח")]
    [SerializeField] private Transform worldLanding;

    [Tooltip("נקודת יעד שאליה ה-Tracker צריך להגיע במהלך המעבר")]
    [SerializeField] private Transform trackerTargetEmpty;

    [Header("Timings")]
    [Tooltip("כמה זמן לחכות אחרי תחילת האנימציה לפני ששולחים את השחקן ללוח")]
    [SerializeField] private float preTeleportDelay = 0.4f;

    [Header("Animation")]
    [SerializeField] private string animationStateName = "Stage_Pass";
    [SerializeField] private float crossFadeTime = 0.05f;

    [Header("Tracker Manual Control (optional)")]
    [SerializeField] private TrackerManualControl trackerControl;

    [Tooltip("קוליידרים לביטול זמני במהלך מעבר")]
    [SerializeField] private Collider[] collidersToDisable;

    [Header("Tracker Follow Settings")]
    [Tooltip("מהירות תנועת הספירה (Tracker)")]
    [SerializeField] private float trackerMoveSpeed = 3f;

    [Tooltip("כמה קרוב ליעד נחשב 'הגענו'")]
    [SerializeField] private float trackerArrivalThreshold = 0.01f;

    private bool triggered = false;
    private bool isHopping = false;
    private Vector3 trackerTarget;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(HopRoutine(other.transform));
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
        // שלב 1: נעל שליטה
        var ctrl = player.GetComponent<ThirdPersonController>();
        var inp = player.GetComponent<StarterAssetsInputs>();
        var mLck = player.GetComponent<MovementLock>();

        if (ctrl) { ctrl.allowZMovementTemporarily = true; ctrl.enabled = false; }
        if (inp) inp.enabled = false;
        if (mLck) mLck.SetExternalLock(true);
        foreach (var c in collidersToDisable) if (c) c.enabled = false;
        if (trackerControl) trackerControl.enabled = false;

        // שלב 2: הפעל אנימציה
        var anim = player.GetComponentInChildren<Animator>();
        if (anim)
            anim.CrossFadeInFixedTime(animationStateName, crossFadeTime, 0, 0f);

        // שלב 3: המתן מעט לפני הטלפורט הראשוני
        yield return new WaitForSeconds(Mathf.Max(preTeleportDelay, crossFadeTime));

        // שלב 4: טלפורט ראשוני אל הלוח
        float yRot = player.eulerAngles.y;
        Warp(player, tracker.position, yRot);

        // שלב 5: תתחיל להזיז את ה-Tracker
        if (trackerTargetEmpty != null)
        {
            trackerTarget = trackerTargetEmpty.position;
            isHopping = true;

            // שלב 6: תזוזת השחקן יחד עם ה-Tracker עד הגעה ליעד
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

        // שלב 7: טלפורט לפאנל הבא (היעד הסופי)
        Warp(player, worldLanding.position, yRot);

        // שלב 8: החזרת שליטה
        isHopping = false;
        if (ctrl) { ctrl.enabled = true; ctrl.allowZMovementTemporarily = false; }
        if (inp) inp.enabled = true;
        if (mLck) mLck.SetExternalLock(false);
        foreach (var c in collidersToDisable) if (c) c.enabled = true;
        if (trackerControl) trackerControl.enabled = true;

        // שלב 9: אפשר לחזור להפעיל את אותו טריגר שוב
        triggered = false;
    }

    static void Warp(Transform t, Vector3 pos, float yRot)
    {
        if (!t) return;

        // ודא שנשמר ציר Z = 0 אם צריך
        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
