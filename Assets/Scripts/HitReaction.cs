using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HitReaction : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] private string triggerName = "Hit";

    private Animator anim;
    private int myId;
    private int triggerHash;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
        triggerHash = Animator.StringToHash(triggerName);
    }

    private void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    private void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;                

        BlockController bc = GetComponent<BlockController>();
        if (bc != null && bc.IsBlocking && IsFacingAttacker(e.attackerId))
            return;                                    

        anim.SetTrigger(triggerHash);                  
    }

    private bool IsFacingAttacker(int attackerId)
    {
        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk))
            return false;

        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
