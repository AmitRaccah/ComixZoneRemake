using UnityEngine;

public class KnifeThrower : MonoBehaviour
{
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwSocket;
    [SerializeField] private string throwAnimationTrigger = "Throw";

    private int playerId;

    private bool pendingThrow;
    private AttackData pendingData;
    private float pendingSpeed, pendingDistance, pendingRotation;

    private Animator anim;

    private void Awake()
    {
        playerId = gameObject.GetInstanceID();
        anim = GetComponent<Animator>();
    }

    private void OnEnable() { CoreBus.Subscribe<KnifeThrownEvent>(OnKnifeThrown); }
    private void OnDisable() { CoreBus.Unsubscribe<KnifeThrownEvent>(OnKnifeThrown); }

    private void OnKnifeThrown(KnifeThrownEvent e)
    {
        if (!CompareTag("Player")) return;

        pendingThrow = true;
        pendingData = e.attackData;
        pendingSpeed = e.speed;
        pendingDistance = e.distance;
        pendingRotation = e.rotationSpeed;

        if (AnimationHelper.Instance) AnimationHelper.Instance.Trigger(throwAnimationTrigger);
        else if (anim) anim.SetTrigger(throwAnimationTrigger);
    }

    // Animation Event
    public void Anim_SpawnKnife()
    {
        if (!pendingThrow || !knifePrefab || pendingData == null) return;

        KnifeFactory.Spawn(gameObject, knifePrefab, throwSocket,
                           pendingData, pendingSpeed, pendingDistance, pendingRotation);

        pendingThrow = false;
        pendingData = null;
    }
}
