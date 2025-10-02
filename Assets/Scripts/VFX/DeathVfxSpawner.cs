using UnityEngine;

[AddComponentMenu("Combat/Health/Death VFX Spawner")]
public class DeathVfxSpawner : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private Vector3 localOffset = Vector3.zero;

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

        if (deathVfxPrefab == null)
            return;

        Vector3 spawnPosition = ParticleEffectUtility.CalculateSpawnPosition(transform.position, transform, localOffset);

        GameObject vfxInstance = Instantiate(deathVfxPrefab, spawnPosition, Quaternion.identity);

        ParticleEffectUtility.DestroyAfterLifetime(vfxInstance, fallbackLifetimeSeconds);
    }
}