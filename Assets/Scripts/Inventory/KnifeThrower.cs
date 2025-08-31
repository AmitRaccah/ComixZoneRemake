using UnityEngine;
using DG.Tweening;

public class KnifeThrower : MonoBehaviour
{
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwSocket;
    [SerializeField] private string throwAnimationTrigger = "ThrowKnife";
    private int playerId;
    private void Awake()
    {
        playerId = gameObject.GetInstanceID();
    }
    private void OnEnable()
    {
        CoreBus.Subscribe<KnifeThrownEvent>(OnKnifeThrown);
    }
    private void OnDisable()
    {
        CoreBus.Unsubscribe<KnifeThrownEvent>(OnKnifeThrown);
    }
    private void OnKnifeThrown(KnifeThrownEvent e)
    {
        if (!CompareTag("Player")) return;
        AnimationHelper.Instance?.Trigger(throwAnimationTrigger);
        Vector3 startPos = throwSocket ? throwSocket.position : transform.position + transform.forward * 0.5f + Vector3.up * 1f;
        GameObject knifeGO = Instantiate(knifePrefab, startPos, Quaternion.LookRotation(transform.forward));
        KnifeProjectile projectile = knifeGO.GetComponent<KnifeProjectile>();
        if (projectile)
        {
            projectile.Initialize(playerId, e.attackData, e.speed, e.distance, e.rotationSpeed);
        }
    }
}