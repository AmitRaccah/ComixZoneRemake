using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;      // Cinemachine 3.x namespace

public class CrossAnime : MonoBehaviour
{
    /* ---------- Animation ---------- */
    [Header("Animation Settings")]
    [SerializeField] private string animationTriggerName = "Pass";
    [SerializeField] private string animationStateName = "Stage_Pass";

    /* ---------- Path ---------- */
    [Header("Path Settings")]
    [Tooltip("Way-points the player walks through (in order)")]
    [SerializeField] private List<Transform> pathPoints = new();

    /* ---------- Movement ---------- */
    [Header("Movement Settings")]
    [SerializeField] private float moveDelay = 0.1f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.05f;

    /* ---------- Camera ---------- */
    [Header("Camera Switch")]
    [Tooltip("Camera that is currently active before the transition")]
    [SerializeField] private CinemachineVirtualCamera currentCam;
    [Tooltip("Camera we want to cut / blend to during the transition")]
    [SerializeField] private CinemachineVirtualCamera targetCam;
    [SerializeField] private int camPriorityActive = 20;
    [SerializeField] private int camPriorityInactive = 5;

    /* ======================================================== */

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StartCoroutine(MoveThroughPathWhenAnimationBegins(other.transform));
    }

    private IEnumerator MoveThroughPathWhenAnimationBegins(Transform player)
    {
        /* ----- 1. Unlock Z movement temporarily ----- */
        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (controller) controller.allowZMovementTemporarily = true;

        /* ----- 2. Kick animation ----- */
        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim) anim.SetTrigger(animationTriggerName);

        /* ----- 3. Wait until state starts ----- */
        yield return new WaitUntil(() =>
            anim && !anim.IsInTransition(0) &&
            anim.GetCurrentAnimatorStateInfo(0).IsName(animationStateName));

        /* ----- 4. Switch camera once, right before movement ----- */
        SwitchToTargetCamera();

        /* ----- 5. Move along path ----- */
        foreach (Transform point in pathPoints)
        {
            float sqStop = stoppingDistance * stoppingDistance;
            float elapsed = 0f;
            const float maxTime = 5f;

            while ((player.position - point.position).sqrMagnitude > sqStop)
            {
                elapsed += Time.deltaTime;
                if (elapsed > maxTime) break;

                Vector3 dir = (point.position - player.position).normalized;
                float step = moveSpeed * Time.deltaTime;
                player.position = Vector3.MoveTowards(player.position, point.position, step);

                if (dir.x != 0 || dir.z != 0)
                    player.forward = new Vector3(dir.x, 0f, dir.z);

                yield return null;
            }

            player.position = point.position; // snap
        }

        /* ----- 6. Lock Z again & done ----- */
        yield return new WaitForSeconds(0.1f);
        if (controller) controller.allowZMovementTemporarily = false;
    }

    /* ======================================================== */
    /* CAMERA HELPER                                            */
    /* ======================================================== */

    private void SwitchToTargetCamera()
    {
        if (targetCam == null) return;         

        if (currentCam) currentCam.Priority = camPriorityInactive;
        targetCam.Priority = camPriorityActive;

        currentCam = targetCam;
        targetCam = null;                     
    }
}
