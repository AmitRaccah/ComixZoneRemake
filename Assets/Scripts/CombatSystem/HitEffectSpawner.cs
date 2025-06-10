// HitEffectSpawner.cs
using UnityEngine;

public class HitEffectSpawner : MonoBehaviour
{
    private int myId;

    private void Awake()
    {
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
        if (e.attackerId == myId || e.targetId == myId)
        {
            var ad = e.attackData;
            if (ad.hitEffectPrefab == null)
                return;

            if (!AttackActivator.TransformsById.TryGetValue(e.attackerId, out Transform attackerTransform))
                return;

            Vector3 spawnPos = attackerTransform.TransformPoint(ad.hitEffectOffset);
            Quaternion spawnRot = attackerTransform.rotation;

            Instantiate(ad.hitEffectPrefab, spawnPos, spawnRot);
        }
    }
}
