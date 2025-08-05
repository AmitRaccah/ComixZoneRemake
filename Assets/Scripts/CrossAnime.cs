using UnityEngine;
using System.Collections;
using DG.Tweening;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHop : MonoBehaviour
{
    /* ───── Main Points ───── */
    [Header("Key Transforms")]
    [SerializeField] private Transform tracker;          // מיקום על הלוח
    [SerializeField] private Transform worldLanding;     // נקודת נחיתה בפאנל-2

    /* ───── Timings & Clip ───── */
    [Header("Timings")]
    [SerializeField] private float preTeleportDelay = 0.4f;   // ← חדש
    [SerializeField] private float stayOnBoardTime = 0.5f;

    [Header("Animation")]
    [SerializeField] private string animationStateName = "Stage_Pass";
    [SerializeField] private float crossFadeTime = 0.05f;

    /* ───── Optional Tracker control ───── */
    [Header("Tracker Manual Control (optional)")]
    [SerializeField] private TrackerManualControl trackerControl;

    /* ───── Optional colliders to disable ───── */
    [SerializeField] private Collider[] collidersToDisable;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(HopRoutine(other.transform));
    }

    IEnumerator HopRoutine(Transform player)
    {
        /* 1. נעל את השחקן */
        var ctrl = player.GetComponent<ThirdPersonController>();
        var inp = player.GetComponent<StarterAssetsInputs>();
        var mLck = player.GetComponent<MovementLock>();

        if (ctrl) { ctrl.allowZMovementTemporarily = true; ctrl.enabled = false; }
        if (inp) inp.enabled = false;
        if (mLck) mLck.SetExternalLock(true);
        foreach (var c in collidersToDisable) if (c) c.enabled = false;

        /* 2. כבה שליטה ידנית ב-Tracker במהלך המעבר */
        if (trackerControl) trackerControl.enabled = false;

        /* 3. הפעל קליפ + המתנה לפני טלפורט */
        var anim = player.GetComponentInChildren<Animator>();
        if (anim)
            anim.CrossFadeInFixedTime(animationStateName, crossFadeTime, 0, 0f);

        yield return new WaitForSeconds(Mathf.Max(preTeleportDelay, crossFadeTime));

        /* 4. טלפורט אל ה-Tracker */
        float yRot = player.eulerAngles.y;
        Warp(player, tracker.position, yRot);

        /* 5. שהייה על הלוח */
        yield return new WaitForSeconds(stayOnBoardTime);

        /* 6. טלפורט לנחיתה בפאנל-2 */
        Warp(player, worldLanding.position, yRot);

        /* 7. שחרר הכול */
        if (ctrl) { ctrl.enabled = true; ctrl.allowZMovementTemporarily = false; }
        if (inp) inp.enabled = true;
        if (mLck) mLck.SetExternalLock(false);
        foreach (var c in collidersToDisable) if (c) c.enabled = true;

        if (trackerControl) trackerControl.enabled = true;

        Destroy(this);
    }

    /* helper */
    static void Warp(Transform t, Vector3 pos, float yRot)
    {
        if (!t) return;
        t.position = pos;
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
