using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HitReaction : MonoBehaviour
{
    [Header("Names")]
    [SerializeField] private string hitTrigger = "Hit";
    [SerializeField] private string attackTag = "Attack";

    Animator anim;
    int myId, hitHash;
    bool queuedHit = false;          // ← דגל

    void Awake()
    {
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
        hitHash = Animator.StringToHash(hitTrigger);
    }

    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;

        var bc = GetComponent<BlockController>();
        if (bc && bc.IsBlocking && IsFacingAttacker(e.attackerId)) return;

        if (anim.GetCurrentAnimatorStateInfo(0).IsTag(attackTag) ||
            (anim.IsInTransition(0) &&
             anim.GetNextAnimatorStateInfo(0).IsTag(attackTag)))
        {
            queuedHit = true;
            return;
        }

        anim.SetTrigger(hitHash);
    }

    public void CheckQueuedHit()
    {
        if (!queuedHit) return;
        queuedHit = false;
        anim.SetTrigger(hitHash);
    }

    bool IsFacingAttacker(int attackerId)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return false;

        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
