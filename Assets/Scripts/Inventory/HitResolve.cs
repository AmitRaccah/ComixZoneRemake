using UnityEngine;

public static class HitResolve
{
    public static void PublishDamageAndFx(
        int attackerId, AttackData data, Collider other,
        Vector3 basePos, Transform basis)
    {
        DoDamage(attackerId, data, other);

        var list = data.additionalHitEffects;
        if (list == null || list.Count == 0) return;

        foreach (var fx in list)
        {
            if (!fx.prefab) continue;

            Vector3 spawnPosition = ParticleEffectUtility.CalculateWorldPosition(basePos, basis, fx.localOffset);
            ParticleEffectUtility.Spawn(fx.prefab, spawnPosition);

            Debug.Log($"[FX] '{fx.prefab.name}' basePos={basePos:F2} pos={spawnPosition:F2} offset={fx.localOffset:F2}");
        }
    }

    static void DoDamage(int attackerId, AttackData data, Collider other)
    {
        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;

        CombatBus.Publish(new DamageEvent
        {
            attackerId = attackerId,
            targetId = root.gameObject.GetInstanceID(),
            amount = data.damage,
            knockback = data.knockback,
            type = data.damageType,
            shakeAmplitude = data.shakeAmplitude,
            freezeFrameDuration = data.freezeFrameDuration,
            attackData = data
        });
    }
}