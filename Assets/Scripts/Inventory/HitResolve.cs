using UnityEngine;

public static class HitResolve
{
    public static void PublishDamageAndFx(
        int attackerId, AttackData data, Collider other,
        Vector3 basePos, Transform basis)
    {
        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;
        bool blocked = BlockUtil.IsBlocked(root, attackerId);

        DoDamage(attackerId, data, root, blocked);

        var list = data.GetHitEffects(blocked);
        if (list == null || list.Count == 0) return;

        foreach (var fx in list)
        {
            if (string.IsNullOrEmpty(fx.vfxId)) continue;

            Vector3 spawnPosition = ParticleEffectUtility.CalculateSpawnPosition(basePos, basis, fx.localOffset);

            VfxPoolManager.Instance.Spawn(fx.vfxId, spawnPosition, Quaternion.identity);
        }
    }


    static void DoDamage(int attackerId, AttackData data, Transform targetRoot, bool isBlocked)
    {
        if (!targetRoot)
            return;

        CombatBus.Publish(new DamageEvent
        {
            attackerId = attackerId,
            targetId = targetRoot.gameObject.GetInstanceID(),
            amount = data.damage,
            knockback = data.knockback,
            type = data.damageType,
            shakeAmplitude = data.GetShakeAmplitude(isBlocked),
            freezeFrameDuration = data.GetFreezeFrameDuration(isBlocked),
            attackData = data,
            isBlocked = isBlocked
        });
    }
}