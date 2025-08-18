using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyCore : MonoBehaviour
{
    public Animator Anim { get; private set; }
    public Rigidbody Body { get; private set; }
    public BehaviorGraphAgent AI { get; private set; }
    [Header("Lane-Lock")]
    [SerializeField] private float laneZ = 0f;
    private int myId;

    void Awake()
    {
        Anim = GetComponent<Animator>();
        Body = GetComponent<Rigidbody>();
        AI = GetComponent<BehaviorGraphAgent>();
        myId = gameObject.GetInstanceID();
    }

    void Start()
    {
        Vector3 p = transform.position;
        p.z = laneZ;
        transform.position = p;
        Body.constraints |= RigidbodyConstraints.FreezePositionZ
                          | RigidbodyConstraints.FreezeRotation;
    }

    void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;
        if (AI != null)
        {
            AI.SetVariableValue("IsStunned", true);
        }
    }

    void Update()
    {
        Health health = GetComponent<Health>();
        if (health != null && !health.IsStunned)
        {
            if (AI != null)
            {
                AI.SetVariableValue("IsStunned", false);
            }
        }
    }
}