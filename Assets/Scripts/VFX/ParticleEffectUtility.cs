using UnityEngine;

public static class ParticleEffectUtility
{
    public static GameObject Spawn(
        GameObject prefab,
        Vector3 basePosition,
        Transform basis,
        Vector3 localOffset,
        bool inheritRotation,
        bool parentToBasis,
        bool autoDestroy,
        float fallbackLifetimeSeconds)
    {
        if (prefab == null)
            return null;

        Vector3 worldPosition = basePosition + GetOffset(localOffset, basis);
        Quaternion worldRotation = GetRotation(inheritRotation, basis);

        GameObject instance = Object.Instantiate(prefab, worldPosition, worldRotation);

        if (parentToBasis && basis != null)
        {
            instance.transform.SetParent(basis, worldPositionStays: true);
        }

        if (autoDestroy)
        {
            var autoDestroyComponent = instance.GetComponent<ParticleEffectAutoDestroy>();
            if (autoDestroyComponent == null)
            {
                autoDestroyComponent = instance.AddComponent<ParticleEffectAutoDestroy>();
            }

            autoDestroyComponent.Begin(fallbackLifetimeSeconds);
        }

        return instance;
    }

    private static Vector3 GetOffset(Vector3 localOffset, Transform basis)
    {
        if (basis == null)
            return localOffset;

        return basis.TransformDirection(localOffset);
    }

    private static Quaternion GetRotation(bool inheritRotation, Transform basis)
    {
        if (!inheritRotation || basis == null)
            return Quaternion.identity;

        return basis.rotation;
    }
}