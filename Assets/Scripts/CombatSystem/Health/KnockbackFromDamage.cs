using UnityEngine;

public class KnockbackFromDamage : MonoBehaviour
{
    private int myId;
    void Awake() => myId = gameObject.GetInstanceID();
    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;
        if (e.knockback <= 0f) return;
        CombatBus.Publish(new KnockbackEvent { targetId = e.targetId, attackerId = e.attackerId, force = e.knockback });
    }
}
