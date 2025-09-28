using UnityEngine;

public static class ParticleEffectUtility
{
    public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        if (!prefab)
        {
            Debug.LogWarning("Tried to spawn a particle effect with a null prefab.");
            return null;
        }

        var effectRotation = rotation ?? Quaternion.identity;
        var instance = Object.Instantiate(prefab, position, effectRotation, parent);

        EnsureAutoDestroy(instance);
        return instance;
    }


    public static Vector3 CalculateWorldPosition(Vector3 basePosition, Transform basis, Vector3 localOffset)
    {
        if (!basis)
        {
            return basePosition + localOffset;
        }

        return basePosition + basis.TransformDirection(localOffset);
    }

    static void EnsureAutoDestroy(GameObject instance)
    {
        if (!instance) return;

        if (!instance.TryGetComponent(out ParticleEffectAutoDestroy autoDestroy))
        {
            autoDestroy = instance.AddComponent<ParticleEffectAutoDestroy>();
        }

        autoDestroy.InitializeFromHierarchy(instance.transform);
    }
}