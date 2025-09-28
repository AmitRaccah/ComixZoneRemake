using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class ParticleEffectAutoDestroy : MonoBehaviour
{
    [Tooltip("Extra delay (in seconds) before destroying the effect after particles have finished.")]
    [SerializeField]
    float additionalDelay = 0f;

    [Tooltip("Recalculate the lifetime every time the effect is enabled.")]
    [SerializeField]
    bool recalculateOnEnable = true;

    Coroutine destroyRoutine;

    void OnEnable()
    {
        if (recalculateOnEnable)
        {
            RestartCountdown();
        }
    }

    void OnDisable()
    {
        if (destroyRoutine != null)
        {
            StopCoroutine(destroyRoutine);
            destroyRoutine = null;
        }
    }

    public void InitializeFromHierarchy(Transform root)
    {
        RestartCountdown(root);
    }

    void RestartCountdown(Transform root = null)
    {
        if (destroyRoutine != null)
        {
            StopCoroutine(destroyRoutine);
        }

        destroyRoutine = StartCoroutine(DestroyWhenComplete(root));
    }

    IEnumerator DestroyWhenComplete(Transform root)
    {
        if (!root)
        {
            root = transform;
        }

        var particleSystems = root.GetComponentsInChildren<ParticleSystem>();
        if (particleSystems.Length == 0)
        {
            yield return new WaitForSeconds(additionalDelay);
            Destroy(gameObject);
            yield break;
        }

        float maxLifetime = 0f;
        foreach (var system in particleSystems)
        {
            if (system == null) continue;
            float lifetime = CalculateParticleSystemLifetime(system);
            maxLifetime = Mathf.Max(maxLifetime, lifetime);
        }

        maxLifetime += additionalDelay;

        yield return new WaitForSeconds(maxLifetime);
        Destroy(gameObject);
    }

    static float CalculateParticleSystemLifetime(ParticleSystem system)
    {
        var main = system.main;
        float startLifetime = ExtractStartLifetime(main.startLifetime);
        float duration = main.duration;

        if (main.loop)
        {
            return Mathf.Max(startLifetime, duration) + 5f;
        }

        float maxLifetime = duration + startLifetime;

        var emission = system.emission;
        if (emission.enabled && emission.burstCount > 0)
        {
            var bursts = new ParticleSystem.Burst[emission.burstCount];
            emission.GetBursts(bursts);
            foreach (var burst in bursts)
            {
                maxLifetime = Mathf.Max(maxLifetime, burst.time + startLifetime);
            }
        }

        return maxLifetime;
    }

    static float ExtractStartLifetime(ParticleSystem.MinMaxCurve lifetimeCurve)
    {
        switch (lifetimeCurve.mode)
        {
            case ParticleSystemCurveMode.Constant:
                return lifetimeCurve.constant;
            case ParticleSystemCurveMode.TwoConstants:
                return lifetimeCurve.constantMax;
            case ParticleSystemCurveMode.Curve:
            case ParticleSystemCurveMode.TwoCurves:
                return lifetimeCurve.constantMax;
            default:
                return lifetimeCurve.constantMax;
        }
    }
}