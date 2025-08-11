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
    public Transform player;                 
    public Animator playerAnimator;          
    public ThirdPersonController controller;
    public CharacterController characterControl;
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;

    [Header("Refs - Tracker / Targets")]
    public Transform tracker;
    public Transform worldLanding;
    public TrackerManualControl trackerCtrl;

    private bool triggered = false;

    // Follow state
    private bool followTracker = false;
    private float followYaw = 0f;

    private void Awake()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        if (requireCrouch)
        {
            StarterAssetsInputs inp = other.GetComponent<StarterAssetsInputs>();
            if (inp == null || !inp.crouch) return;
        }

        StartTimeline();
    }

    private void StartTimeline()
    {
        if (director == null) return;

        triggered = true;
        director.time = 0.0;
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null)
        {
            d.stopped -= OnDirectorStopped;
        }
        triggered = false;
    }

    /* ---------- -Signal Track ---------- */

    public void LockPlayer()
    {
        if (controller != null)
        {
            controller.allowZMovementTemporarily = true;
            controller.enabled = false;
            characterControl.enabled = false;
        }

        if (inputs != null)
        {
            inputs.enabled = false;
        }

        if (movementLock != null)
        {
            movementLock.SetExternalLock(true);
        }

        if (trackerCtrl != null)
        {
            trackerCtrl.enabled = false;
        }
    }

    public void TeleportToTracker()
    {
        if (player == null || tracker == null) return;
        float yRot = player.eulerAngles.y;
        SafeTeleport(player, tracker.position, yRot);
    }

    public void StartFollowTracker()
    {
        if (player != null)
        {
            followYaw = player.eulerAngles.y;
        }
        followTracker = true;
    }

    public void StopFollowTracker()
    {
        followTracker = false;
    }

    public void TeleportToWorld()
    {
        if (player == null || worldLanding == null) return;
        float yRot = player.eulerAngles.y;
        SafeTeleport(player, worldLanding.position, yRot);
    }

    public void ReleaseControl()
    {
        if (movementLock != null)
        {
            movementLock.SetExternalLock(false);
        }

        if (inputs != null)
        {
            inputs.enabled = true;
        }

        if (controller != null)
        {
            controller.enabled = true;
            characterControl.enabled = true;
            controller.allowZMovementTemporarily = false;
        }

        if (trackerCtrl != null)
        {
            trackerCtrl.enabled = true;
        }
    }

    public void StopTimeline()
    {
        if (director == null) return;
        director.Stop();
    }

    /* ---------- Follow tracker ---------- */

    private void LateUpdate()
    {
        if (followTracker && player != null && tracker != null)
        {
            SafeTeleport(player, tracker.position, followYaw);
        }
    }

    /* ---------- Utils ---------- */

    private void SafeTeleport(Transform t, Vector3 pos, float yRot)
    {
        if (t == null) return;

        CharacterController cc = null;
        if (controller != null)
        {
            cc = controller.GetComponent<CharacterController>();
        }

        if (cc != null)
        {
            cc.enabled = false;
        }

        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);

        if (cc != null)
        {
            cc.enabled = true;
        }
    }
}
