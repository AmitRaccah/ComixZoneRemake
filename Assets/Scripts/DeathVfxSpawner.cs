using System.Collections;
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

    private void OnValidate()
    {
        if (fallbackLifetimeSeconds < 0f)
            fallbackLifetimeSeconds = 0f;
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

        Vector3 basePosition = transform.position;
        Vector3 spawnPosition = basePosition + transform.TransformDirection(localOffset);

        GameObject vfxInstance = Instantiate(deathVfxPrefab, spawnPosition, Quaternion.identity);

        StartCoroutine(DestroyWhenFinished(vfxInstance));
    }

    private IEnumerator DestroyWhenFinished(GameObject vfxInstance)
    {
        float waitTime = CalculateLifetime(vfxInstance);
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

        if (vfxInstance != null)
        {
            Destroy(vfxInstance);
        }
    }

    private float CalculateLifetime(GameObject vfxInstance)
    {
        float maxLifetime = 0f;
        ParticleSystem[] particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            ParticleSystem system = particleSystems[i];
            ParticleSystem.MainModule main = system.main;

            if (main.loop)
            {
                return 0f;
            }

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

        return maxLifetime;
    }
}