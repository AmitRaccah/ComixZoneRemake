// DeathVfxSpawner.cs
using UnityEngine;

[AddComponentMenu("Combat/Health/Death VFX Spawner")]
public class DeathVfxSpawner : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private ParticleEffectData deathVfx;

    [Header("Lifetime")]
    [Tooltip("Fallback lifetime used when the spawned VFX has no finite particle duration.")]
    [Min(0f)]
    [SerializeField] private float fallbackLifetimeSeconds = 5f;

    private int entityId;

    private void Awake()
    {
        entityId = gameObject.GetInstanceID();
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnValidate()
    {
        if (fallbackLifetimeSeconds < 0f)
            fallbackLifetimeSeconds = 0f;
    }

    private void OnDeath(HealthDepletedEvent e)
    {
        if (e.entityId != entityId)
            return;

        if (VfxPoolManager.Instance == null)
            return;

        if (deathVfx == null || string.IsNullOrEmpty(deathVfx.vfxId))
            return;

        Vector3 spawnPosition = ParticleEffectUtility.CalculateSpawnPosition(
            transform.position, transform, deathVfx.localOffset
        );

        GameObject spawned = VfxPoolManager.Instance.Spawn(
            deathVfx.vfxId, spawnPosition, transform.rotation
        );
        if (!spawned) return;

        var member = spawned.GetComponent<VfxPoolMember>();
        if (!member) return;

        ParticleEffectUtility.ReturnAfterLifetime(member, fallbackLifetimeSeconds);
    }
}
