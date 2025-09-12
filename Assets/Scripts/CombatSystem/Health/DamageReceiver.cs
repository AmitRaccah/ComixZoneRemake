using UnityEngine;

[RequireComponent(typeof(Health))]
public class DamageReceiver : MonoBehaviour
{
    private Health health;
    private int myId;

    private void Awake()
    {
        health = GetComponent<Health>();
        myId = gameObject.GetInstanceID();
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void OnDamage(DamageEvent e)
    {
        if (health == null) return;
        if (health.IsDead) return;
        if (e.targetId != myId) return;

        bool blocked = false;
        BlockController bc = GetComponent<BlockController>();
        if (bc != null && bc.IsBlocking && IsFacingAttacker(e.attackerId))
        {
            blocked = true;
        }
        if (blocked) return;

        health.TakeDamage(e.amount, e.attackerId, e.knockback);
    }

    private bool IsFacingAttacker(int attackerId)
    {
        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk))
        {
            return false;
        }
        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
