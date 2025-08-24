using UnityEngine;
using UnityEngine.Playables;
using StarterAssets;

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
    public Transform emptyTarget;
    public Transform worldLanding;

    private bool triggered = false;

    void Awake()
    {
        if (director == null) director = GetComponent<PlayableDirector>();
        if (director != null) director.extrapolationMode = DirectorWrapMode.None;
    }

    void OnDisable()
    {
        if (director != null) director.stopped -= OnDirectorStopped;
        SafeReleaseControl();
    }

    void OnTriggerEnter(Collider other)
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

    void StartTimeline()
    {
        if (director == null) return;
        triggered = true;

        if (playerAnimator != null) playerAnimator.applyRootMotion = true;

        director.time = 0.0;
        director.stopped += OnDirectorStopped;
        director.Play();
    }

    void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null) d.stopped -= OnDirectorStopped;
        triggered = false;
        SafeReleaseControl();
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

    public void TeleportToTracker()
    {
        if (player == null || emptyTarget == null) return;
        float yRot = player.eulerAngles.y;
        SafeTeleport(player, emptyTarget.position, yRot);
    }

    public void TeleportToWorld()
    {
        if (player == null || worldLanding == null) return;
        float yRot = player.eulerAngles.y;
        SafeTeleport(player, worldLanding.position, yRot);
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

    void SafeTeleport(Transform t, Vector3 pos, float yRot)
    {
        if (t == null) return;

        CharacterController cc = null;
        if (controller != null) cc = controller.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        t.position = pos;
        t.rotation = Quaternion.Euler(0f, yRot, 0f);

        if (cc != null) cc.enabled = true;
    }

    void SafeReleaseControl()
    {
        if (movementLock != null) movementLock.SetExternalLock(false);
        if (inputs != null) inputs.enabled = true;
        if (controller != null) controller.allowZMovementTemporarily = false;
    }
}
