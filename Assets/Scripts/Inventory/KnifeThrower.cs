using UnityEngine;

public class KnifeThrower : MonoBehaviour
{
    [SerializeField] private Transform throwSocket;
    [SerializeField] private string throwAnimationTrigger = "Throw";

    private int playerId;
    private bool pendingThrow;
    private string pendingKnifeId;
    private AttackData pendingData;
    private float pendingSpeed, pendingDistance, pendingRotation;
    private Animator anim;

    void Awake()
    {
        playerId = gameObject.GetInstanceID();
        anim = GetComponent<Animator>();
    }

    void OnEnable() { CoreBus.Subscribe<KnifeThrownEvent>(OnKnifeThrown); }
    void OnDisable() { CoreBus.Unsubscribe<KnifeThrownEvent>(OnKnifeThrown); }

    void OnKnifeThrown(KnifeThrownEvent e)
    {
        if (!CompareTag("Player")) return;
        pendingThrow = true;
        pendingKnifeId = e.knifeId;
        pendingData = e.attackData;
        pendingSpeed = e.speed;
        pendingDistance = e.distance;
        pendingRotation = e.rotationSpeed;
        if (AnimationHelper.Instance) AnimationHelper.Instance.Trigger(throwAnimationTrigger);
        else if (anim) anim.SetTrigger(throwAnimationTrigger);
    }

    public void Anim_SpawnKnife()
    {
        if (!pendingThrow || string.IsNullOrEmpty(pendingKnifeId) || pendingData == null) return;
        KnifeFactory.Spawn(gameObject, pendingKnifeId, throwSocket, pendingData, pendingSpeed, pendingDistance, pendingRotation);
        pendingThrow = false;
        pendingKnifeId = null;
        pendingData = null;
    }
}
