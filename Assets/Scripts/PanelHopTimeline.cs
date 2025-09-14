using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHopTimeline : MonoBehaviour
{
    public PlayableDirector director;
    public bool requireCrouch = false;
    public string playerTag = "Player";
    public Transform player;
    public Animator playerAnimator;
    public ThirdPersonController controller;
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;
    public Transform worldLanding;
    public TrackerFollowDeltaX trackerFollow;
    public string enterRoomId;

    private bool triggered = false;
    private bool playerInsideGate = false;
    private static PanelHopTimeline waitingCrouchGate = null;

    void Awake()
    {
        if (director != null) director.extrapolationMode = DirectorWrapMode.None;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        player = other.transform;
        playerAnimator = other.GetComponent<Animator>();
        controller = other.GetComponent<ThirdPersonController>();
        inputs = other.GetComponent<StarterAssetsInputs>();
        movementLock = other.GetComponent<MovementLock>();
        if (inputs == null) return;

        if (requireCrouch)
        {
            if (inputs.crouch) StartTimeline();
            else { playerInsideGate = true; waitingCrouchGate = this; }
        }
        else
        {
            StartTimeline();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInsideGate = false;
        if (waitingCrouchGate == this) waitingCrouchGate = null;
    }

    void Update()
    {
        if (playerInsideGate && waitingCrouchGate == this && inputs != null && inputs.crouch && !triggered)
        {
            playerInsideGate = false;
            waitingCrouchGate = null;
            StartTimeline();
        }
    }

    void StartTimeline()
    {
        if (director == null) return;
        triggered = true;

        ResolvePlayerIfNeeded();
        RebindNullBindings();

        if (playerAnimator != null) playerAnimator.applyRootMotion = true;

        director.time = 0f;
        director.Evaluate();
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null) d.stopped -= OnDirectorStopped;
        triggered = false;
        SafeReleaseControl();

        if (trackerFollow != null)
        {
            if (!string.IsNullOrEmpty(enterRoomId)) trackerFollow.ApplyRoom(enterRoomId);
            trackerFollow.ResetSync();
        }
    }

    public void EnableRootMotion()
    {
        if (playerAnimator != null) playerAnimator.applyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        if (playerAnimator != null) playerAnimator.applyRootMotion = false;
    }

    public void LockPlayer()
    {
        if (movementLock != null) movementLock.SetExternalLock(true);
        if (inputs != null) inputs.enabled = false;
        if (controller != null) controller.allowZMovementTemporarily = true;
    }

    public void TeleportToWorld()
    {
        if (player == null || worldLanding == null) return;
        CharacterController cc = controller != null ? controller.GetComponent<CharacterController>() : null;
        if (cc != null) cc.enabled = false;
        player.position = worldLanding.position;
        if (cc != null) cc.enabled = true;
        if (trackerFollow != null) trackerFollow.ResetSync();
    }

    public void ReleaseControl()
    {
        SafeReleaseControl();
    }

    public void StopTimeline()
    {
        if (director == null) return;
        director.Stop();
    }

    void SafeReleaseControl()
    {
        if (movementLock != null) movementLock.SetExternalLock(false);
        if (inputs != null) inputs.enabled = true;
        if (controller != null) controller.allowZMovementTemporarily = false;
    }

    void ResolvePlayerIfNeeded()
    {
        if (player != null && playerAnimator != null && inputs != null && controller != null && movementLock != null) return;

        var go = GameObject.FindGameObjectWithTag(playerTag);
        if (!go) return;

        player = go.transform;
        playerAnimator = go.GetComponent<Animator>();
        controller = go.GetComponent<ThirdPersonController>();
        inputs = go.GetComponent<StarterAssetsInputs>();
        movementLock = go.GetComponent<MovementLock>();
    }

    void RebindNullBindings()
    {
        if (director == null || director.playableAsset == null) return;
        if (player == null || playerAnimator == null) return;

        foreach (var output in director.playableAsset.outputs)
        {
            var key = output.sourceObject;
            var curr = director.GetGenericBinding(key);
            if (curr != null) continue;

            var t = output.outputTargetType;
            if (t == typeof(Animator))
                director.SetGenericBinding(key, playerAnimator);
            else if (t == typeof(GameObject) || t == typeof(Transform))
                director.SetGenericBinding(key, player.gameObject);
        }

        director.RebuildGraph();
    }
}
