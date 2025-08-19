using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PanelHopTimelineController : MonoBehaviour
{
    public PlayableDirector director;

    [Header("Gate Settings")]
    public bool requireCrouch = false;
    public string playerTag = "Player";

    public Transform player;
    public Animator playerAnimator;
    public ThirdPersonController controller;
    public CharacterController characterControl;
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;

    public Transform tracker;
    public Transform worldLanding;
    public TrackerManualControl trackerCtrl;

    private bool triggered = false;
    private bool followTracker = false;
    private float followYaw = 0f;

    private float initialYaw = 0f;
    private Vector3 initialPosition = Vector3.zero;
    private float yRot;

    // Fix: event-driven crouch gating (no cross-gate memory)
    private bool playerInsideGate = false;
    private static PanelHopTimelineController waitingCrouchGate = null;

    private void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<PlayerCrouchEvent>(OnCrouchEvent);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<PlayerCrouchEvent>(OnCrouchEvent);
        if (waitingCrouchGate == this) waitingCrouchGate = null;
        playerInsideGate = false;
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (triggered) yield break;
        if (!other.CompareTag(playerTag)) yield break;

        if (requireCrouch)
        {
            // Arm this gate and wait for crouch via EventBus (no coroutine wait here)
            playerInsideGate = true;
            waitingCrouchGate = this;
            yield break;
        }

        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
            initialPosition = player.position;
        }
        StartTimeline();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        // Leaving cancels arming for this gate only
        playerInsideGate = false;
        if (waitingCrouchGate == this)
            waitingCrouchGate = null;
    }

    // Called globally from StarterAssetsInputs via CoreBus when crouch is pressed
    private void OnCrouchEvent(PlayerCrouchEvent _)
    {
        if (!requireCrouch) return;
        if (triggered) return;                 // already running a timeline
        if (!playerInsideGate) return;         // must still be inside this gate
        if (waitingCrouchGate != this) return; // only the armed gate can fire

        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
            initialPosition = player.position;
        }
        StartTimeline();
    }

    public void EnableRootMotion()
    {
        if (playerAnimator != null)
            playerAnimator.applyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        if (playerAnimator != null)
            playerAnimator.applyRootMotion = false;
    }

    private void StartTimeline()
    {
        if (director == null) return;

        triggered = true;

        // Clear any pending crouch from any gate to avoid cross-gate memory
        waitingCrouchGate = null;
        playerInsideGate = false;

        director.time = 0.0;
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    public void ResetAnimatorPose()
    {
        if (playerAnimator != null && player != null)
        {
            playerAnimator.Rebind();
            player.rotation = Quaternion.Euler(0f, initialYaw, 0f);
            player.position = initialPosition;
        }
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null)
            d.stopped -= OnDirectorStopped;

        // Safety: ensure root motion is OFF when the timeline ends
        DisableRootMotion();

        triggered = false;
    }

    public void LockPlayer()
    {
        if (movementLock != null)
            movementLock.SetExternalLock(true);

        if (inputs != null)
            inputs.enabled = false;

        if (trackerCtrl != null)
            trackerCtrl.enabled = false;

        if (controller != null)
            controller.allowZMovementTemporarily = true;
    }

    public void TeleportToTracker()
    {
        if (player == null || tracker == null) return;

        yRot = Mathf.Approximately(player.eulerAngles.y, 0f) ? initialYaw : player.eulerAngles.y;
        SafeTeleport(player, tracker.position, yRot);
    }

    public void StartFollowTracker()
    {
        if (player != null)
            followYaw = player.eulerAngles.y;

        followTracker = true;
    }

    public void StopFollowTracker()
    {
        followTracker = false;
    }

    public void TeleportToWorld()
    {
        if (player == null || worldLanding == null) return;

        float rot = player.eulerAngles.y;
        SafeTeleport(player, worldLanding.position, rot);
    }

    public void ReleaseControl()
    {
        if (movementLock != null)
            movementLock.SetExternalLock(false);

        if (inputs != null)
            inputs.enabled = true;

        if (trackerCtrl != null)
        {
            trackerCtrl.enabled = true;
            trackerCtrl.SetLimitsForCollider(GetComponent<Collider>());
        }

        if (controller != null)
            controller.allowZMovementTemporarily = false;
    }

    public void StopTimeline()
    {
        if (director == null) return;
        director.Stop();
    }

    private void LateUpdate()
    {
        if (followTracker && player != null && tracker != null)
            SafeTeleport(player, tracker.position, followYaw);
    }

    private void SafeTeleport(Transform t, Vector3 pos, float yRot)
    {
        if (t == null) return;

        CharacterController cc = null;
        if (controller != null)
            cc = controller.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        t.position = pos;
        t.rotation = Quaternion.Euler(0f, yRot, 0f);

        if (cc != null)
            cc.enabled = true;
    }
}
