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
    private Vector3 initialPosition = Vector3.zero; // משתנה חדש לשמירת המיקום
    float yRot;

    private void Awake()
    {
        if (director == null)
        {
            director = GetComponent<PlayableDirector>();
        }
        Debug.Log("Awake called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter started, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
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
            initialPosition = player.position; // שמור את המיקום ההתחלתי
            Debug.Log("Initial position saved: " + initialPosition + ", initialYaw: " + initialYaw);
        }
        StartTimeline();
    }

    private void BeginTimeline(Transform player, StarterAssetsInputs inputs)
    {
        Debug.Log("BeginTimeline called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (player != null)
        {
            initialYaw = player.eulerAngles.y;
            initialPosition = player.position; // שמור את המיקום ההתחלתי
            Debug.Log("Initial position saved: " + initialPosition + ", initialYaw: " + initialYaw);
        }
        StartTimeline();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        triggered = true; // עוצר את ההמתנה אם השחקן יוצא מהקוליידר
        Debug.Log("OnTriggerExit called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void EnableRootMotion()
    {
        Debug.Log("EnableRootMotion called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (playerAnimator != null)
            playerAnimator.applyRootMotion = true;
    }

    public void DisableRootMotion()
    {
        Debug.Log("DisableRootMotion called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (playerAnimator != null)
            playerAnimator.applyRootMotion = false;
    }

    private void StartTimeline()
    {
        Debug.Log("StartTimeline called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (director == null) return;
        triggered = true;
        director.time = 0.0;
        director.stopped += OnDirectorStopped;
        director.Play();
        Debug.Log("Timeline started, player position after play: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void ResetAnimatorPose()
    {
        Debug.Log("ResetAnimatorPose called, initialYaw: " + initialYaw + ", initialPosition: " + initialPosition + ", current position: " + (player ? player.position : "null") + ", current rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (playerAnimator != null)
        {
            playerAnimator.Rebind();
            Debug.Log("After Rebind, position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
            player.rotation = Quaternion.Euler(0f, initialYaw, 0f);
            player.position = initialPosition; // שחזר את המיקום ההתחלתי
            Debug.Log("After position and rotation restore, position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        }
    }

    private void OnDirectorStopped(PlayableDirector d)
    {
        Debug.Log("OnDirectorStopped called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (d != null)
        {
            d.stopped -= OnDirectorStopped;
        }
        triggered = false;
    }

    public void LockPlayer()
    {
        Debug.Log("LockPlayer called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
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
        Debug.Log("After LockPlayer, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void TeleportToTracker()
    {
        Debug.Log("TeleportToTracker called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
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
        Debug.Log("After TeleportToTracker, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void StartFollowTracker()
    {
        Debug.Log("StartFollowTracker called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (player != null)
        {
            followYaw = player.eulerAngles.y;
        }
        followTracker = true;
    }

    public void StopFollowTracker()
    {
        Debug.Log("StopFollowTracker called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        followTracker = false;
    }

    public void TeleportToWorld()
    {
        Debug.Log("TeleportToWorld called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (player == null || worldLanding == null) return;
        float yRot = player.eulerAngles.y;
        SafeTeleport(player, worldLanding.position, yRot);
        Debug.Log("After TeleportToWorld, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void ReleaseControl()
    {
        Debug.Log("ReleaseControl called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
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
        Debug.Log("After ReleaseControl, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
    }

    public void StopTimeline()
    {
        Debug.Log("StopTimeline called, player position: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        if (director == null) return;
        director.Stop();
    }

    private void LateUpdate()
    {
        if (followTracker && player != null && tracker != null)
        {
            Debug.Log("LateUpdate follow, player position before: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
            SafeTeleport(player, tracker.position, followYaw);
            Debug.Log("LateUpdate follow, player position after: " + (player ? player.position : "null") + ", rotation: " + (player ? player.eulerAngles : Vector3.zero));
        }
    }

    private void SafeTeleport(Transform t, Vector3 pos, float yRot)
    {
        if (t == null) return;
        Debug.Log("SafeTeleport called, target pos: " + pos + ", yRot: " + yRot + ", current position: " + (t ? t.position : "null") + ", current rotation: " + (t ? t.eulerAngles : Vector3.zero));
        CharacterController cc = null;
        if (controller != null)
        {
            cc = controller.GetComponent<CharacterController>();
        }
        if (cc != null)
        {
            cc.enabled = false;
            Debug.Log("CharacterController disabled, position: " + (t ? t.position : "null"));
        }
        t.position = new Vector3(pos.x, pos.y, pos.z);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
        Debug.Log("After set position and rotation, position: " + (t ? t.position : "null") + ", rotation: " + (t ? t.eulerAngles : Vector3.zero));
        if (cc != null)
        {
            cc.enabled = true;
            Debug.Log("CharacterController enabled, position: " + (t ? t.position : "null"));
        }
    }
}