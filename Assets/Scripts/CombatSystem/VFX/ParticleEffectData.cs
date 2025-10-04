using UnityEngine;
using System.Collections;

[System.Serializable]
public class ParticleEffectData
{
    [Tooltip("The ID of the VFX to spawn from the VfxPoolManager.")]
    public string vfxId;
    public Vector3 localOffset = Vector3.zero;
}

public static class ParticleEffectUtility
{
    public static Vector3 CalculateSpawnPosition(Vector3 basePosition, Transform basis, Vector3 localOffset)
    {
        return basis ? basePosition + basis.TransformDirection(localOffset) : basePosition + localOffset;
    }

    public static float CalculateLifetime(GameObject vfxInstance)
    {
        if (!vfxInstance)
            return 0f;

        float maxLifetime = 0f;
        var particleSystems = vfxInstance.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var system = particleSystems[i];
            if (!system) continue;

            var main = system.main;
            if (main.loop)
                return 0f;

            float systemLifetime = main.duration;
            var startLifetime = main.startLifetime;

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
                maxLifetime = systemLifetime;
        }

        return maxLifetime;
    }

    public static void ReturnAfterLifetime(VfxPoolMember member, float fallbackLifetimeSeconds)
    {
        if (!member) return;
        member.StartCoroutine(ReturnRoutine(member, fallbackLifetimeSeconds));
    }

    private static IEnumerator ReturnRoutine(VfxPoolMember member, float fallbackLifetimeSeconds)
    {
        var go = member.gameObject;
        float waitTime = CalculateLifetime(go);
        if (waitTime <= 0f)
            waitTime = fallbackLifetimeSeconds;

        if (waitTime > 0f)
            yield return new WaitForSeconds(waitTime);

        if (VfxPoolManager.Instance != null)
            VfxPoolManager.Instance.Return(member);
    }
}
