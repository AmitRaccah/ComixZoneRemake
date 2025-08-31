using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HitReaction : MonoBehaviour
{
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string attackTag = "Attack";

    private Animator anim;
    private int myId;
    private int hitHash;
    private bool queuedHit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
        hitHash = Animator.StringToHash(hitTrigger);
    }

    private void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    private void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;

        var bc = GetComponent<BlockController>();
        if (bc && bc.IsBlocking && IsFacingAttacker(e.attackerId)) return;

        anim.ResetTrigger(hitHash);
        anim.SetTrigger(hitHash);

        if (IsInAttack()) queuedHit = true;
    }

    public void CheckQueuedHit()
    {
        if (!queuedHit) return;
        queuedHit = false;
        anim.ResetTrigger(hitHash);
        anim.SetTrigger(hitHash);
    }

    private bool IsInAttack()
    {
        var s = anim.GetCurrentAnimatorStateInfo(0);
        if (s.IsTag(attackTag)) return true;
        if (anim.IsInTransition(0))
        {
            var n = anim.GetNextAnimatorStateInfo(0);
            if (n.IsTag(attackTag)) return true;
        }
        return false;
    }

    private bool IsFacingAttacker(int attackerId)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return false;

        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
