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
    float yRot;

    private void Awake()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (triggered) yield break;
        if (!other.CompareTag(playerTag)) yield break;
        if (requireCrouch)
        {
            StarterAssetsInputs inp = other.GetComponent<StarterAssetsInputs>();
            Debug.Log("Crouch input detected initial: " + (inp != null && inp.crouch));
            if (inp == null) yield break;
            if (inp.crouch)
            {
                BeginTimeline(other.transform, inp);
                yield break;
            }
            while (!triggered && inp != null && !inp.crouch)
            {
                yield return null;
            }
            if (inp.crouch)
            {
                BeginTimeline(other.transform, inp);
            }
            yield break;
        }
        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
        }
        StartTimeline();
    }

    private void BeginTimeline(Transform player, StarterAssetsInputs inputs)
    {
        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
        }
        StartTimeline();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        triggered = true; // עוצר את ההמתנה אם השחקן יוצא מהקוליידר
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
        director.time = 0.0;
        director.stopped += OnDirectorStopped;
        director.Play();
        Debug.Log("Starting Timeline for transition"); // לוג להצגת הפעלת ה-Timeline
    }

    public void ResetAnimatorPose()
    {
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            player.rotation = Quaternion.Euler(0f, initialYaw, 0f);
        }
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        if (d != null)
        {
            d.stopped -= OnDirectorStopped;
        }
        triggered = false;
    }

    public void LockPlayer()
    {
        if (movementLock != null)
        {
            movementLock.SetExternalLock(true);
        }
        if (inputs != null)
        {
            inputs.enabled = false;
        }
        if (trackerCtrl != null)
        {
            trackerCtrl.enabled = false;
        }
        if (controller != null)
        {
            controller.allowZMovementTemporarily = true;
        }
    }

    public void TeleportToTracker()
    {
        if (player == null || tracker == null) return;
        if (player.eulerAngles.y == 0)
        {
            yRot = initialYaw;
        }
        else
        {
            yRot = player.eulerAngles.y;
        }
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
        if (trackerCtrl != null)
        {
            trackerCtrl.enabled = true;
            trackerCtrl.SetLimitsForCollider(GetComponent<Collider>());
        }
        if (controller != null)
        {
            controller.allowZMovementTemporarily = false;
        }
    }

    public void StopTimeline()
    {
        if (director == null) return;
        director.Stop();
    }

    private void LateUpdate()
    {
        if (followTracker && player != null && tracker != null)
        {
            SafeTeleport(player, tracker.position, followYaw);
        }
    }

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
        t.position = new Vector3(pos.x, pos.y, pos.z);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
        if (cc != null)
        {
            cc.enabled = true;
        }
    }
}