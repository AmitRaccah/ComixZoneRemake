using UnityEngine;
using System.Collections;
using DG.Tweening;
using Unity.Cinemachine;     // Cinemachine 3.x

public class CrossAnime : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private string animationTriggerName = "Pass";
    [SerializeField] private string animationStateName = "Stage_Pass";

    [Header("Movement")]
    [SerializeField] private Transform pathPoint;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float endDelay = 0.1f;

    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera currentCam;
    [SerializeField] private CinemachineVirtualCamera targetCam;
    [SerializeField] private int camPriorityActive = 20;
    [SerializeField] private int camPriorityInactive = 5;

    [Header("Wall Collider")]
    [SerializeField] private Collider wallCollider;

    private bool _triggered = false;

    private Transform _player;

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;

        _triggered = true;
        StartCoroutine(DoStagePass(other.transform));
    }

    private IEnumerator DoStagePass(Transform player)
    {
        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
        var input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
        var mLock = player.GetComponent<MovementLock>();

        if (controller != null)
        {
            controller.allowZMovementTemporarily = true;
            controller.enabled = false;
        }
        if (input != null) input.enabled = false;
        if (mLock != null) mLock.SetExternalLock(true);
        if (wallCollider != null) wallCollider.enabled = false;

        var anim = player.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger(animationTriggerName);

        yield return new WaitUntil(
            delegate
            {
                if (anim == null) return true;   
                bool playingDesired =
                    !anim.IsInTransition(0) &&
                    anim.GetCurrentAnimatorStateInfo(0).IsName(animationStateName);
                return playingDesired;
            });

        if (targetCam != null)
        {
            if (currentCam != null) currentCam.Priority = camPriorityInactive;
            targetCam.Priority = camPriorityActive;

            currentCam = targetCam;
            targetCam = null;
        }

        if (pathPoint != null)
        {
            _player = player;                   

            float distance = Vector3.Distance(_player.position, pathPoint.position);
            float duration = distance / moveSpeed;

            Tween tween = _player
                .DOMove(pathPoint.position, duration)
                .SetEase(Ease.Linear)           
                .OnUpdate(OrientPlayer);        

            yield return tween.WaitForCompletion();

            _player = null;                   
        }

        yield return new WaitForSeconds(endDelay);

        if (controller != null)
        {
            controller.enabled = true;
            controller.allowZMovementTemporarily = false;
        }
        if (input != null) input.enabled = true;
        if (mLock != null) mLock.SetExternalLock(false);
        if (wallCollider != null) wallCollider.enabled = true;

        Destroy(this); 
    }

    private void OrientPlayer()
    {
        if (_player == null || pathPoint == null) return;

        Vector3 direction = (pathPoint.position - _player.position).normalized;
       // _player.forward = new Vector3(direction.x, 0f, direction.z);
    }
}
