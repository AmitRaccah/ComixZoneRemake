using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HitReaction : MonoBehaviour
{
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string attackTag = "Attack";

    private Animator anim;
    private BlockController bc;
    private int myId;
    private int hitHash;
    private bool wasBlocking;

    private bool isDead = false;

    private const string HitStateName = "TakeHit";

    private void Awake()
    {
        anim = GetComponent<Animator>();
        bc = GetComponent<BlockController>();
        myId = gameObject.GetInstanceID();
        hitHash = Animator.StringToHash(hitTrigger);
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
        CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
        CoreBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
        CoreBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
    }

    private void Update()
    {
        bool nowBlocking = bc && bc.IsBlocking;
        if (nowBlocking && !wasBlocking)
            anim.ResetTrigger(hitHash);
        wasBlocking = nowBlocking;
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;
        if (isDead) return;                               
        if (e.isBlocked) return;
        if (bc && bc.IsBlocking && IsFacingAttacker(e.attackerId)) return;

        anim.ResetTrigger(hitHash);
        anim.Play(HitStateName, 0, 0f);
        anim.Update(0f);
    }

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;
        isDead = true;
        anim.ResetTrigger(hitHash);
    }

    private void OnHealthChanged(HealthChangedEvent e)
    {
        if (e.entityId != myId) return;
        if (!e.isDead && e.current > 0) isDead = false;   
    }

    public void CheckQueuedHit() { }

    private bool IsFacingAttacker(int attackerId)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk)) return false;
        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
