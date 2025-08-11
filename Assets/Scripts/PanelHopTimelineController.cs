using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHopTimelineController : MonoBehaviour
{
    [Header("Playable Director")]
    public PlayableDirector director;

    [Header("Gate Settings")]
    public bool requireCrouch = false;
    public string playerTag = "Player";

    [Header("Refs - Player (ROOT עם CC)")]
    public Transform player;                 // ה־Root עם CharacterController
    public Animator playerAnimator;          // ה־Animator על PlayerArmature (אם צריך)
    public ThirdPersonController controller; // קומפוננט התנועה
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;

    [Header("Refs - Tracker / Targets")]
    public Transform tracker;
    public Transform worldLanding;
    public TrackerManualControl trackerCtrl;

    private bool triggered = false;

    void Awake()
    {
        if (!director) director = GetComponent<PlayableDirector>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        if (requireCrouch)
        {
            var inp = other.GetComponent<StarterAssetsInputs>();
            if (inp == null || !inp.crouch) return;
        }

        StartTimeline();
    }

    void StartTimeline()
    {
        if (!director) return;
        triggered = true;
        director.time = 0;
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        d.stopped -= OnDirectorStopped;
        triggered = false;
    }

    /* ---------- נקראות מה-Signal Track ---------- */

    public void LockPlayer()
    {
        if (controller) { controller.allowZMovementTemporarily = true; controller.enabled = false; }  // השבתת שליטה
        if (inputs) inputs.enabled = false;
        if (movementLock) movementLock.SetExternalLock(true);
        if (trackerCtrl) trackerCtrl.enabled = false;
    }

    public void TeleportToTracker()
    {
        if (!player || !tracker) return;
        SafeTeleport(player, tracker.position, player.eulerAngles.y);
    }

    public void StartFollowTracker()
    {
        if (player) _followYaw = player.eulerAngles.y;
        _followTracker = true;
    }

    public void StopFollowTracker()
    {
        _followTracker = false;
    }

    public void TeleportToWorld()
    {
        if (!player || !worldLanding) return;
        SafeTeleport(player, worldLanding.position, player.eulerAngles.y);
    }

    public void ReleaseControl()
    {
        if (movementLock) movementLock.SetExternalLock(false);
        if (inputs) inputs.enabled = true;
        if (controller) { controller.enabled = true; controller.allowZMovementTemporarily = false; }
        if (trackerCtrl) trackerCtrl.enabled = true;
    }

    // חיתוך סופי של הטיים־ליין (מומלץ לשים סיגנל פריים אחרי TeleportToWorld)
    public void StopTimeline()
    {
        if (!director) return;
        director.Stop();
    }

    /* ---------- Follow לטרקר ---------- */

    bool _followTracker = false;
    float _followYaw = 0f;

    void LateUpdate()
    {
        if (_followTracker && player && tracker)
            SafeTeleport(player, tracker.position, _followYaw);
    }

    /* ---------- Utils ---------- */

    // טלפורט בטוח: מכבה לרגע את CharacterController כדי שלא "יתקן" מיקום
    void SafeTeleport(Transform t, Vector3 pos, float yRot)
    {
        if (!t) return;
        var cc = controller ? controller.GetComponent<CharacterController>() : null;
        if (cc) cc.enabled = false;

        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);

        if (cc) cc.enabled = true;
    }
}
