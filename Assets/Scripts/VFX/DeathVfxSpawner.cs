using UnityEngine;

[AddComponentMenu("Combat/Health/Death VFX Spawner")]
public class DeathVfxSpawner : MonoBehaviour
{
    [Header("VFX Settings")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Vector3 localOffset = Vector3.zero;
    [SerializeField] private bool inheritSpawnRotation = true;

    [Header("Lifetime")]
    [Tooltip("Fallback lifetime used when the spawned VFX has no finite particle duration.")]
    [Min(0f)]
    [SerializeField] private float fallbackLifetimeSeconds = 5f;

    private int entityId;

    private void Awake()
    {
        entityId = gameObject.GetInstanceID();
    }

    private void Reset()
    {
        spawnPoint = transform;
    }

    private void OnValidate()
    {
        if (fallbackLifetimeSeconds < 0f)
            fallbackLifetimeSeconds = 0f;

        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDeath);
    }

    private void OnDeath(HealthDepletedEvent e)
    {
        if (e.entityId != entityId)
            return;

        if (deathVfxPrefab == null)
            return;

        Transform basis = spawnPoint != null ? spawnPoint : transform;
        Vector3 basePosition = basis.position;

        ParticleEffectUtility.Spawn(
            deathVfxPrefab,
            basePosition,
            basis,
            localOffset,
            inheritSpawnRotation,
            parentToBasis: true,
            autoDestroy: true,
            fallbackLifetimeSeconds: fallbackLifetimeSeconds);
    }
}
Assets / Scripts / CombatSystem / VFX / ParticleEffectAutoDestroy.cs
New
+ 88 - 0
using System.Collections;
using UnityEngine;

public class ParticleEffectAutoDestroy : MonoBehaviour
{
    [SerializeField]
    [Min(0f)]
    private float fallbackLifetimeSeconds = 0f;

    private bool coroutineStarted;

    public void Begin(float fallbackSeconds)
    {
        fallbackLifetimeSeconds = Mathf.Max(0f, fallbackSeconds);

        if (coroutineStarted)
            return;

        StartCoroutine(DestroyWhenFinished());
        coroutineStarted = true;
    }

    private IEnumerator DestroyWhenFinished()
    {
        float waitTime = CalculateLifetime();

        if (waitTime <= 0f)
        {
            waitTime = fallbackLifetimeSeconds;
        }

        if (waitTime > 0f)
        {
            yield return new WaitForSeconds(waitTime);
        }
        else
        {
            yield return null;
        }

        if (this != null && gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    private float CalculateLifetime()
    {
        float maxLifetime = 0f;
        bool foundFiniteSystem = false;

        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem system = particleSystems[i];
            ParticleSystem.MainModule main = system.main;

            if (main.loop)
            {
                return 0f;
            }

            foundFiniteSystem = true;

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

        return foundFiniteSystem ? maxLifetime : 0f;
    }
}