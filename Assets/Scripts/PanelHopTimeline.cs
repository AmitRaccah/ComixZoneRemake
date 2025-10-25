using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

[RequireComponent(typeof(Collider))]
public class PanelHopTimeline : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector director;

    [Header("Gate")]
    public bool requireCrouch = false;
    public string playerTag = "Player";

    [Header("Player Refs")]
    public Transform player;
    public Animator playerAnimator;
    public ThirdPersonController controller;
    public StarterAssetsInputs inputs;
    public MovementLock movementLock;

    [Header("World / Tracker")]
    public Transform worldLanding;
    public TrackerFollowDeltaX trackerFollow;
    public string enterRoomId;

    [Header("Facing")]
    public float entryYaw = 90f;
    public float finalYaw = -90f;

    private bool triggered = false;
    private bool playerInsideGate = false;
    private static PanelHopTimeline waitingCrouchGate = null;
    private bool waitingForGrounded = false;
    private bool pendingFinalFacing = false;

    void Awake()
    {
        if (director != null) director.extrapolationMode = DirectorWrapMode.None;
    }

    void OnDisable()
    {
        if (director != null) director.stopped -= OnDirectorStopped;
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag(playerTag)) return;

        StarterAssetsInputs inp = other.GetComponent<StarterAssetsInputs>();
        if (inp == null) return;
        inputs = inp;

        if (requireCrouch)
        {
            if (inputs.crouch)
            {
                StartTimeline();
            }
            else
            {
                playerInsideGate = true;
                waitingCrouchGate = this;
            }
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
        waitingForGrounded = false;
    }

    void Update()
    {
        if (playerInsideGate && waitingCrouchGate == this && inputs != null && inputs.crouch && !triggered)
        {
            playerInsideGate = false;
            waitingCrouchGate = null;
            StartTimeline();
        }

        if (waitingForGrounded && controller != null && controller.Grounded && !triggered)
        {
            StartTimeline();
        }

        if (pendingFinalFacing && controller != null && controller.Grounded)
        {
            pendingFinalFacing = false;
            DoFinalFacingNow();
        }
    }

    void StartTimeline()
    {
        if (director == null)
        {
            waitingForGrounded = false;
            return;
        }

        if (controller != null && !controller.Grounded)
        {
            waitingForGrounded = true;
            return;
        }

        waitingForGrounded = false;
        triggered = true;

        ResetAllInputs();
        ClearCombatBuffer();

        ApplyEntryFacing();

        if (playerAnimator != null) playerAnimator.applyRootMotion = true;

        director.time = 0f;
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null) d.stopped -= OnDirectorStopped;
        triggered = false;
        waitingForGrounded = false;

        DisableRootMotion();
        ApplyFinalFacing();
        SafeReleaseControl();
        ResetAllInputs();
        ClearCombatBuffer();

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
        ResetAllInputs();
        ClearCombatBuffer();
    }

    public void TeleportToWorld()
    {
        if (player == null || worldLanding == null) return;

        CharacterController cc = null;
        if (controller != null) cc = controller.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = worldLanding.position;

        if (cc != null) cc.enabled = true;

        ApplyFinalFacing();

        if (trackerFollow != null) trackerFollow.ResetSync();
    }

    public void ReleaseControl()
    {
        SafeReleaseControl();
    }

    public void StopTimeline()
    {
        waitingForGrounded = false;
        if (director == null) return;
        director.Stop();
    }

    void SafeReleaseControl()
    {
        if (movementLock != null) movementLock.SetExternalLock(false);
        if (inputs != null) inputs.enabled = true;
        if (controller != null) controller.allowZMovementTemporarily = false;
    }

    void ResetAllInputs()
    {
        if (inputs == null) return;
        inputs.move = Vector2.zero;
        inputs.look = Vector2.zero;
        inputs.jump = false;
        inputs.sprint = false;
        inputs.crouch = false;
        inputs.block = false;
        inputs.pickUp = false;
        inputs.lookUp = false;
        inputs.analogMovement = false;
    }

    void ClearCombatBuffer()
    {
        if (InputBuffer.Instance != null) InputBuffer.Instance.Clear();
    }

    void ApplyEntryFacing()
    {
        if (player != null) player.rotation = Quaternion.Euler(0f, entryYaw, 0f);
        if (playerAnimator != null)
        {
            bool faceRight = Mathf.Abs(Mathf.DeltaAngle(entryYaw, 90f)) < 1f;
            playerAnimator.SetBool("Mirror", !faceRight);
        }
    }

    void ApplyFinalFacing()
    {
        if (controller != null && !controller.Grounded)
        {
            pendingFinalFacing = true;
            return;
        }

        DoFinalFacingNow();
    }

    void DoFinalFacingNow()
    {
        if (player != null) player.rotation = Quaternion.Euler(0f, finalYaw, 0f);
        if (playerAnimator != null)
        {
            bool faceRight = Mathf.Abs(Mathf.DeltaAngle(finalYaw, 90f)) < 1f;
            playerAnimator.SetBool("Mirror", !faceRight);
        }
    }
}
