using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class PanelHopTimelineController : MonoBehaviour
{
    public PlayableDirector director;
    public bool requireCrouch = false;
    public string playerTag = "Player";
    public Transform player;
    public Animator playerAnimator;
    public ThirdPersonController controller;
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;
    public Transform tracker;
    public Transform worldLanding;
    public TrackerFollowDeltaX trackerFollow;
    public string enterRoomId;

    private bool triggered = false;
    private bool followTracker = false;
    private float followYaw = 0f;
    private float initialYaw = 0f;
    private Vector3 initialPosition = Vector3.zero;
    private float yRot = 0f;
    private bool playerInsideGate = false;
    private static PanelHopTimelineController waitingCrouchGate = null;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
    }

    void OnEnable()
    {
        CoreBus.Subscribe<PlayerCrouchEvent>(OnCrouchEvent);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<PlayerCrouchEvent>(OnCrouchEvent);
        if (waitingCrouchGate == this) waitingCrouchGate = null;
        playerInsideGate = false;
    }

    IEnumerator OnTriggerEnter(Collider other)
    {
        if (triggered) yield break;
        if (!other.CompareTag(playerTag)) yield break;

        if (requireCrouch)
        {
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

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInsideGate = false;
        if (waitingCrouchGate == this) waitingCrouchGate = null;
    }

    void OnCrouchEvent(PlayerCrouchEvent _)
    {
        if (!requireCrouch) return;
        if (triggered) return;
        if (!playerInsideGate) return;
        if (waitingCrouchGate != this) return;

        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
            initialPosition = player.position;
        }
        StartTimeline();
    }

    public void EnableRootMotion()
    {
        if (playerAnimator != null) playerAnimator.applyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        if (playerAnimator != null) playerAnimator.applyRootMotion = false;
    }

    void StartTimeline()
    {
        if (director == null) return;
        triggered = true;
        waitingCrouchGate = null;
        playerInsideGate = false;
        director.time = 0.0;
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

    public void LockPlayer()
    {
        if (movementLock != null) movementLock.SetExternalLock(true);
        if (inputs != null) inputs.enabled = false;
        if (controller != null) controller.allowZMovementTemporarily = true;
        if (trackerFollow != null) trackerFollow.enabled = false;
    }

    public void TeleportToTracker()
    {
        if (player == null || tracker == null) return;
        yRot = Mathf.Approximately(player.eulerAngles.y, 0f) ? initialYaw : player.eulerAngles.y;
        SafeTeleport(player, tracker.position, yRot);
    }

    public void StartFollowTracker()
    {
        if (player != null) followYaw = player.eulerAngles.y;
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
        if (movementLock != null) movementLock.SetExternalLock(false);
        if (inputs != null) inputs.enabled = true;
        if (controller != null) controller.allowZMovementTemporarily = false;

        if (trackerFollow != null)
        {
            if (!string.IsNullOrEmpty(enterRoomId)) trackerFollow.ApplyRoom(enterRoomId);
            trackerFollow.enabled = true;
            trackerFollow.ResetSync();
        }
    }

    public void StopTimeline()
    {
        if (director == null) return;
        director.Stop();
    }

    void LateUpdate()
    {
        if (followTracker && player != null && tracker != null)
        {
            SafeTeleport(player, tracker.position, followYaw);
        }
    }

    void SafeTeleport(Transform t, Vector3 pos, float yaw)
    {
        if (t == null) return;
        CharacterController cc = null;
        if (controller != null) cc = controller.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        t.position = pos;
        t.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cc != null) cc.enabled = true;
    }
}
