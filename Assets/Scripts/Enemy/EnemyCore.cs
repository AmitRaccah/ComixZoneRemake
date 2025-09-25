using UnityEngine;
using Unity.Behavior;


public class EnemyCore : MonoBehaviour
{
    public Animator Anim { get; private set; }
    public Rigidbody Body { get; private set; }
    public BehaviorGraphAgent AI { get; private set; }

    [Header("Lane-Lock")]
    [SerializeField] private float laneZ = 0f;

    private int myId;
    private EnemyCombatState combatState;

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Body = GetComponent<Rigidbody>();
        AI = GetComponent<BehaviorGraphAgent>();
        myId = gameObject.GetInstanceID();
        combatState = GetComponent<EnemyCombatState>();
    }

    private void Start()
    {
        Vector3 p = transform.position;
        p.z = laneZ;
        transform.position = p;

        Body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
        Body.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionZ;
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void FixedUpdate()
    {
        Vector3 v = Body.linearVelocity;
        if (v.z != 0f)
        {
            v.z = 0f;
            Body.linearVelocity = v;
        }

        Vector3 pos = Body.position;
        if (pos.z != laneZ)
        {
            pos.z = laneZ;
            Body.position = pos;
        }
    }

    private void LateUpdate()
    {
        float y = transform.eulerAngles.y;
        float d90 = Mathf.Abs(Mathf.DeltaAngle(y, 90f));
        float dm90 = Mathf.Abs(Mathf.DeltaAngle(y, -90f));
        float target = (d90 <= dm90) ? 90f : -90f;
        transform.rotation = Quaternion.Euler(0f, target, 0f);
    }

    private void Update()
    {
        if (AI != null && combatState != null)
        {
            AI.SetVariableValue<bool>("GotHit", combatState.GotHit);
            AI.SetVariableValue<bool>("HitRecently", combatState.HitRecently);
            AI.SetVariableValue<bool>("IsBeingSpammed", combatState.IsBeingSpammed);
        }

        var stun = GetComponent<HitStunController>();
        if (stun != null && !stun.IsStunned) { AI?.SetVariableValue("IsStunned", false); }
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;

        if (combatState != null) combatState.RegisterHit();
        if (AI != null) AI.SetVariableValue("IsStunned", true);

        if (AttackActivator.TransformsById.TryGetValue(e.attackerId, out var atk))
        {
            Vector3 toAtt = (atk.position - transform.position).normalized;
            bool fromBehind = Vector3.Dot(transform.forward, toAtt) < 0f;
            bool attackerOnRight = (atk.position.x - transform.position.x) > 0f;

            AI?.SetVariableValue("HitFromBehind", fromBehind);
            AI?.SetVariableValue("AttackerOnRight", attackerOnRight);
        }
    }
}
