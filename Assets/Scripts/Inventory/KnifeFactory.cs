using UnityEngine;

public static class KnifeFactory
{
    public static void Spawn(GameObject owner, GameObject prefab, Transform socket,
                             AttackData data, float speed, float distance, float rotationSpeed)
    {
        if (owner == null || prefab == null || data == null) return;

        Vector3 startPos = socket
            ? socket.position
            : owner.transform.position + owner.transform.forward * 0.5f + Vector3.up * 1f;

        Quaternion rot = Quaternion.LookRotation(owner.transform.forward);

        var go = Object.Instantiate(prefab, startPos, rot);
        go.GetComponent<KnifeProjectile>()?.Initialize(
            owner.GetInstanceID(), data, speed, distance, rotationSpeed
        );
    }
}
