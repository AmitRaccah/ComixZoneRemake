using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CrossAnime : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private string animationTriggerName = "Pass";
    [Tooltip("This should match the exact name of the state in the Animator")]
    [SerializeField] private string animationStateName = "Stage_Pass";

    [Header("Path Settings")]
    [Tooltip("List of path points the player should walk through (in order)")]
    [SerializeField] private List<Transform> pathPoints = new();

    [Header("Movement Settings")]
    [SerializeField] private float moveDelay = 0.1f;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.05f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        StartCoroutine(MoveThroughPathWhenAnimationBegins(other.transform));
    }

    private IEnumerator MoveThroughPathWhenAnimationBegins(Transform player)
    {
        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
        if (controller != null)
            controller.allowZMovementTemporarily = true;

        Animator anim = player.GetComponentInChildren<Animator>();
        if (anim != null)
            anim.SetTrigger(animationTriggerName);

        yield return new WaitUntil(() =>
            anim != null &&
            !anim.IsInTransition(0) &&
            anim.GetCurrentAnimatorStateInfo(0).IsName(animationStateName)
        );

        foreach (Transform point in pathPoints)
        {
            float elapsed = 0f;
            float maxTime = 5f; // allow more time if needed

            while ((player.position - point.position).sqrMagnitude > stoppingDistance * stoppingDistance)
            {
                elapsed += Time.deltaTime;
                if (elapsed > maxTime) break;

                Vector3 direction = (point.position - player.position).normalized;
                float step = moveSpeed * Time.deltaTime;
                player.position = Vector3.MoveTowards(player.position, point.position, step);

                if (direction.x != 0 || direction.z != 0)
                    player.forward = new Vector3(direction.x, 0, direction.z);

                yield return null;
            }

            // Optional: Snap to exact point at the end of segment
            player.position = point.position;
        }

        // 🔒 Now we verify final position before locking Z movement again
        Transform finalTarget = pathPoints[^1]; // last in list

        // Wait until player is truly at the last point (for safety)
        yield return new WaitUntil(() =>
            (player.position - finalTarget.position).sqrMagnitude <= stoppingDistance * stoppingDistance
        );

        // Short buffer time
        yield return new WaitForSeconds(0.1f);

        if (controller != null)
            controller.allowZMovementTemporarily = false;
    }

}
