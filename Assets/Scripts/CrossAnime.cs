using UnityEngine;
using System.Collections;
using Unity.Cinemachine;      // Cinemachine 3.x namespace

public class CrossAnime : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string animationTriggerName = "Pass";
    [SerializeField] private string animationStateName = "Stage_Pass";

    [SerializeField] private Transform pathPoint;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.05f;

    [Header("Camera Switch")]
    [SerializeField] private CinemachineVirtualCamera currentCam;
    [SerializeField] private CinemachineVirtualCamera targetCam;
    [SerializeField] private int camPriorityActive = 20;
    [SerializeField] private int camPriorityInactive = 5;

    [Header("Wall Collider")]
    [SerializeField] private Collider wallCollider;

    private Coroutine _moveRoutine;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _moveRoutine != null) return;
        _moveRoutine = StartCoroutine(MoveToPointSequence(other.transform));
    }

    private IEnumerator MoveToPointSequence(Transform player)
    {
        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
        {
            controller.allowZMovementTemporarily = true;
            controller.enabled = false;
        }

        if (wallCollider != null)
            wallCollider.enabled = false;

        var anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(animationTriggerName);

        yield return new WaitUntil(() =>
            anim != null &&
            !anim.IsInTransition(0) &&
            anim.GetCurrentAnimatorStateInfo(0).IsName(animationStateName)
        );

        if (targetCam != null)
        {
            if (currentCam != null) currentCam.Priority = camPriorityInactive;
            targetCam.Priority = camPriorityActive;
            currentCam = targetCam;
            targetCam = null;
        }

        if (pathPoint != null)
        {
            float sqStop = stoppingDistance * stoppingDistance;
            while ((player.position - pathPoint.position).sqrMagnitude > sqStop)
            {
                Vector3 dir = (pathPoint.position - player.position).normalized;
                player.position = Vector3.MoveTowards(
                    player.position,
                    pathPoint.position,
                    moveSpeed * Time.deltaTime
                );

                player.forward = new Vector3(dir.x, 0f, dir.z);
                yield return null;
            }
            player.position = pathPoint.position;
        }

        yield return new WaitForSeconds(0.1f);

        if (controller != null)
        {
            controller.enabled = true;
            controller.allowZMovementTemporarily = false;
        }

        if (wallCollider != null)
            wallCollider.enabled = true;

        Destroy(this);
    }
}
