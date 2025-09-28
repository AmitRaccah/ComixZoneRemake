using UnityEngine;

public static class HitResolve
{
    public static void PublishDamageAndFx(
        int attackerId, AttackData data, Collider other,
        Vector3 basePos, Transform basis)
    {
        Transform targetRoot = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;

        DoDamage(attackerId, data, targetRoot);

        var list = data.additionalHitEffects;
        if (list == null || list.Count == 0) return;

        foreach (var fx in list)
        {
            if (!fx.prefab) continue;

            Vector3 spawnPosition = ParticleEffectUtility.CalculateSpawnPosition(basePos, basis, fx.localOffset);

            Object.Instantiate(fx.prefab, spawnPosition, Quaternion.identity);
        }
    }

    static void DoDamage(int attackerId, AttackData data, Transform targetRoot)
    {
        if (!targetRoot) return;

        CombatBus.Publish(new DamageEvent
        {
            attackerId = attackerId,
            targetId = targetRoot.gameObject.GetInstanceID(),
            amount = data.damage,
            knockback = data.knockback,
            type = data.damageType,
            shakeAmplitude = data.shakeAmplitude,
            freezeFrameDuration = data.freezeFrameDuration,
            attackData = data
        });
    }
}