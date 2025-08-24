using UnityEngine;
using System.Collections;
using StarterAssets;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider))]
public class PanelHop : MonoBehaviour
{
    [SerializeField] private Transform tracker;
    [SerializeField] private Transform worldLanding;
    [SerializeField] private Transform trackerTargetEmpty;

    [SerializeField] private float preTeleportDelay = 0.4f;
    [SerializeField] private float preTrackerMovementDelay = 0.3f;

    [SerializeField] private string animationStateName = "Stage_Pass";

    [SerializeField] private TrackerFollowDeltaX trackerFollow;

    [SerializeField] private float trackerMoveSpeed = 3f;

    [SerializeField] private Transform trackerEndPoint;
    [SerializeField] private float trackerEndMoveSpeed = 2f;

    [SerializeField] private bool requireCrouch = false;

    [SerializeField] private CinemachineCamera currentCamera;
    [SerializeField] private CinemachineCamera nextCamera;

    [SerializeField] private string enterRoomId;

    private bool triggered = false;
    private bool isHopping = false;
    private bool didSwitchCameras = false;
    private Vector3 trackerTarget;
    private Coroutine crouchWaitRoutine = null;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        StarterAssetsInputs input = other.GetComponent<StarterAssetsInputs>();
        if (input == null) return;

        if (requireCrouch)
        {
            if (input.crouch)
            {
                BeginHop(other.transform, input);
            }
            else
            {
                crouchWaitRoutine = StartCoroutine(WaitForCrouch(other.transform, input));
            }
        }
        else
        {
            BeginHop(other.transform, input);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (crouchWaitRoutine != null)
        {
            StopCoroutine(crouchWaitRoutine);
            crouchWaitRoutine = null;
        }
    }

    IEnumerator WaitForCrouch(Transform player, StarterAssetsInputs input)
    {
        while (!triggered)
        {
            if (input == null || player == null) yield break;
            if (input.crouch)
            {
                BeginHop(player, input);
                yield break;
            }
            yield return null;
        }
    }

    void BeginHop(Transform player, StarterAssetsInputs input)
    {
        triggered = true;

        if (requireCrouch)
        {
            CoreBus.Publish(new PlayerUncrouchEvent());
            input.crouch = false;
        }

        StartCoroutine(HopRoutine(player));
    }

    void Update()
    {
        if (isHopping)
        {
            tracker.position = Vector3.MoveTowards(
                tracker.position,
                trackerTarget,
                trackerMoveSpeed * Time.deltaTime
            );
        }
    }

    IEnumerator HopRoutine(Transform player)
    {
        ThirdPersonController ctrl = player.GetComponent<ThirdPersonController>();
        StarterAssetsInputs inp = player.GetComponent<StarterAssetsInputs>();
        MovementLock mLck = player.GetComponent<MovementLock>();

        if (ctrl != null)
        {
            ctrl.allowZMovementTemporarily = true;
            ctrl.enabled = false;
        }
        if (inp != null) inp.enabled = false;
        if (mLck != null) mLck.SetExternalLock(true);

        if (trackerFollow != null) trackerFollow.enabled = false;

        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsCrouching", false);
            anim.Play(animationStateName, 0, 0f);
        }

        yield return new WaitForSeconds(preTeleportDelay);

        float yRot = player.eulerAngles.y;
        Warp(player, tracker.position, yRot);

        if (!didSwitchCameras) SwitchToNextCamera();

        if (trackerTargetEmpty != null)
        {
            trackerTarget = trackerTargetEmpty.position;

            yield return new WaitForSeconds(preTrackerMovementDelay);
            isHopping = true;

            while (tracker.position != trackerTarget)
            {
                Warp(player, tracker.position, yRot);
                yield return null;
            }
        }

        Warp(player, worldLanding.position, yRot);

        isHopping = false;

        if (ctrl != null)
        {
            ctrl.enabled = true;
            ctrl.allowZMovementTemporarily = false;
        }
        if (inp != null) inp.enabled = true;
        if (mLck != null) mLck.SetExternalLock(false);

        triggered = false;

        if (trackerEndPoint != null)
        {
            StartCoroutine(MoveTrackerToEndPoint());
        }
        else
        {
            if (trackerFollow != null)
            {
                trackerFollow.ApplyRoom(enterRoomId);
                trackerFollow.enabled = true;
                trackerFollow.ResetSync();
            }
        }
    }

    IEnumerator MoveTrackerToEndPoint()
    {
        Vector3 endTarget = trackerEndPoint.position;

        while (tracker.position != endTarget)
        {
            tracker.position = Vector3.MoveTowards(
                tracker.position,
                endTarget,
                trackerEndMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (trackerFollow != null)
        {
            trackerFollow.ApplyRoom(enterRoomId);
            trackerFollow.enabled = true;
            trackerFollow.ResetSync();
        }
    }

    void SwitchToNextCamera()
    {
        if (nextCamera == null) return;

        if (!nextCamera.gameObject.activeSelf) nextCamera.gameObject.SetActive(true);
        if (nextCamera.Follow == null) nextCamera.Follow = tracker;
        if (nextCamera.LookAt == null) nextCamera.LookAt = tracker;

        if (currentCamera != null && currentCamera.gameObject.activeSelf)
        {
            currentCamera.gameObject.SetActive(false);
        }

        didSwitchCameras = true;
    }

    static void Warp(Transform t, Vector3 pos, float yRot)
    {
        if (t == null) return;
        t.position = new Vector3(pos.x, pos.y, 0f);
        t.rotation = Quaternion.Euler(0f, yRot, 0f);
    }
}
