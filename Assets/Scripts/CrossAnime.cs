//using UnityEngine;
//using System.Collections;
//using DG.Tweening;
//using Unity.Cinemachine;    // Cinemachine 3.x
//public class CrossAnime : MonoBehaviour
//{
//    /* ───── Animation ───── */
//    [Header("Animation")]
//    [SerializeField] private string animationStateName = "Stage_Pass";
//    [SerializeField] private float crossFadeTime = 0.05f;   // blend time
//    /* ───── Movement ───── */
//    [Header("Movement")]
//    [SerializeField] private Transform pathPoint;
//    [SerializeField] private float moveSpeed = 2.5f;
//    [SerializeField] private float endDelay = 0.1f;
//    /* ───── Camera ───── */
//    [Header("Camera")]
//    [SerializeField] private CinemachineCamera currentCam;
//    [SerializeField] private CinemachineCamera targetCam;
//    [SerializeField] private int camPriorityActive = 20;
//    [SerializeField] private int camPriorityInactive = 5;
//    /* ───── Colliders to disable during pass ───── */
//    [Header("Colliders")]
//    [SerializeField] private Collider[] collidersToDisable;
//    /* ───── Down‑Pass Settings ───── */
//    [Header("Down Pass")]
//    [SerializeField] private bool requireCrouch = false;
//    /* ───────── State ───────── */
//    bool _triggered;
//    bool _playerInside;
//    Coroutine _waitRoutine;
//    /* ───────── Trigger Entry ───────── */
//    void OnTriggerEnter(Collider other)
//    {
//        if (_triggered || !other.CompareTag("Player")) return;
//        _playerInside = true;
//        var input = other.GetComponent<StarterAssets.StarterAssetsInputs>();
//        if (input == null) return;
//        if (!requireCrouch)
//        {
//            BeginPass(other.transform, input);
//            return;
//        }
//        if (input.crouch)
//            BeginPass(other.transform, input);
//        else
//            _waitRoutine = StartCoroutine(WaitForCrouch(input, other.transform));
//    }
//    /* ───────── Trigger Exit ───────── */
//    void OnTriggerExit(Collider other)
//    {
//        if (!other.CompareTag("Player")) return;
//        _playerInside = false;
//        if (!_triggered && _waitRoutine != null)
//        {
//            StopCoroutine(_waitRoutine);
//            _waitRoutine = null;
//        }
//    }
//    /* ───────── Wait until crouch is pressed ───────── */
//    IEnumerator WaitForCrouch(StarterAssets.StarterAssetsInputs input, Transform player)
//    {
//        while (!_triggered && _playerInside)
//        {
//            if (input.crouch)
//            {
//                BeginPass(player, input);
//                yield break;
//            }
//            yield return null;
//        }
//    }
//    /* ───────── Begin Pass ───────── */
//    void BeginPass(Transform player, StarterAssets.StarterAssetsInputs input)
//    {
//        _triggered = true;
//        if (requireCrouch && input.crouch)
//        {
//            CoreBus.Publish(new PlayerUncrouchEvent());
//            input.crouch = false;
//        }
//        var toggler = GetComponent<TriggerDelayedActivator>();
//        if (toggler) toggler.BeginTimers();
//        StartCoroutine(DoStagePass(player));
//    }
//    /* ───────── Main Coroutine ───────── */
//    IEnumerator DoStagePass(Transform player)
//    {
//        var controller = player.GetComponent<StarterAssets.ThirdPersonController>();
//        var input = player.GetComponent<StarterAssets.StarterAssetsInputs>();
//        var movementLock = player.GetComponent<MovementLock>();
//        if (controller)
//        {
//            controller.allowZMovementTemporarily = true;
//            controller.enabled = false;
//        }
//        if (input) input.enabled = false;
//        if (movementLock) movementLock.SetExternalLock(true);
//        foreach (var col in collidersToDisable)
//            if (col) col.enabled = false;
//        var anim = player.GetComponentInChildren<Animator>();
//        if (anim)
//        {
//            anim.SetBool("IsCrouching", false);               // safety
//            anim.CrossFadeInFixedTime(animationStateName, crossFadeTime, 0, 0f);
//        }
//        yield return new WaitForSeconds(crossFadeTime);
//        if (targetCam)
//        {
//            targetCam.gameObject.SetActive(true);
//            targetCam.enabled = true;
//            if (currentCam)
//            {
//                currentCam.enabled = false;
//                currentCam.gameObject.SetActive(false);
//                currentCam.Priority = camPriorityInactive;
//            }
//            targetCam.Priority = camPriorityActive;
//            currentCam = targetCam;
//            targetCam = null;
//        }
//        if (pathPoint)
//        {
//            float dist = Vector3.Distance(player.position, pathPoint.position);
//            float dur = dist / moveSpeed;
//            yield return player.DOMove(pathPoint.position, dur)
//                               .SetEase(Ease.Linear)
//                               .WaitForCompletion();
//        }
//        yield return new WaitForSeconds(endDelay);
//        if (controller)
//        {
//            controller.enabled = true;
//            controller.allowZMovementTemporarily = false;
//        }
//        if (input) input.enabled = true;
//        if (movementLock) movementLock.SetExternalLock(false);
//        foreach (var col in collidersToDisable)
//            if (col) col.enabled = true;
//        Destroy(this);
//    }
//}











