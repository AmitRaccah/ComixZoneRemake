using UnityEngine;

[AddComponentMenu("VFX/Death VFX Spawner")]
public class DeathVfxSpawner : MonoBehaviour
{
    [Header("Setup")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Lifetime")]
    [Tooltip("Used when the spawned VFX has no finite duration to determine when to destroy it.")]
    [Min(0f)]
    [SerializeField] private float fallbackLifetimeSeconds = 5f;

    private int entityId;

    private void Reset()
    {
        spawnPoint = transform;
    }

    private void Awake()
    {
        entityId = gameObject.GetInstanceID();

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    private void OnValidate()
    {
        if (fallbackLifetimeSeconds < 0f)
        {
            fallbackLifetimeSeconds = 0f;
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnDeath(HealthDepletedEvent evt)
    {
        if (evt.entityId != entityId)
            return;

        if (deathVfxPrefab == null || spawnPoint == null)
            return;

        Transform origin = spawnPoint;
        GameObject instance = Instantiate(
            deathVfxPrefab,
            origin.position,
            origin.rotation,
            origin);

        float lifetime = CalculateLifetime(instance);
        if (lifetime <= 0f)
        {
            lifetime = fallbackLifetimeSeconds;
        }

        if (lifetime > 0f)
        {
            Destroy(instance, lifetime);
        }
        else
        {
            Destroy(instance);
        }
    }

    private static float CalculateLifetime(GameObject instance)
    {
        if (instance == null)
            return 0f;

        ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
        if (particleSystems == null || particleSystems.Length == 0)
            return 0f;

        float maxLifetime = 0f;
        bool hasFiniteSystem = false;

        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem system = particleSystems[i];
            ParticleSystem.MainModule main = system.main;

            if (main.loop)
            {
                return 0f;
            }

            hasFiniteSystem = true;

            float systemLifetime = main.duration;
            ParticleSystem.MinMaxCurve startLifetime = main.startLifetime;

            switch (startLifetime.mode)
            {
                case ParticleSystemCurveMode.Constant:
                    systemLifetime += startLifetime.constant;
                    break;
                case ParticleSystemCurveMode.TwoConstants:
                    systemLifetime += startLifetime.constantMax;
                    break;
                default:
                    systemLifetime += startLifetime.constantMax;
                    break;
            }

            if (systemLifetime > maxLifetime)
            {
                maxLifetime = systemLifetime;
            }
        }

        return hasFiniteSystem ? maxLifetime : 0f;
    }
}