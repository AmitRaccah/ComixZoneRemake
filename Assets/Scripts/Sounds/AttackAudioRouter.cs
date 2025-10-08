using UnityEngine;

public class AttackAudioRouter : MonoBehaviour
{
    [SerializeField] private SfxPlayer sfx;

    void OnEnable()
    {
        CombatBus.Subscribe<AttackStartedEvent>(OnAttackStarted);
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<AttackStartedEvent>(OnAttackStarted);
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    void OnAttackStarted(AttackStartedEvent e)
    {
        if (sfx == null || e.attack == null) return;
        var cue = e.attack.swingSfx;
        if (cue == null) return;

        Vector3 pos = e.socket ? e.socket.position : GetEntityPos(e.attackerId);
        sfx.Play(cue, pos, e.socket);
    }

    void OnDamage(DamageEvent e)
    {
        if (sfx == null || e.attackData == null) return;

        var cue = e.attackData.GetImpactCue(e.isBlocked);
        if (cue == null) return;

        Vector3 pos = GetEntityPos(e.targetId);
        sfx.Play(cue, pos, null);
    }

    Vector3 GetEntityPos(int id)
    {
        if (AttackActivator.TransformsById.TryGetValue(id, out var t) && t)
            return t.position;

        if (FactionId.Transforms.TryGetValue(id, out var t2) && t2)
            return t2.position;

        return Vector3.zero;
    }
}
