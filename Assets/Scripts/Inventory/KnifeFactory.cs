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

        startPos.z = owner.transform.position.z;

        float sign = Mathf.Sign(Vector3.Dot(
            socket ? socket.forward : owner.transform.forward,
            Vector3.right
        ));
        if (sign == 0f) sign = 1f;

        Vector3 dir = new Vector3(sign, 0f, 0f);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        var go = Object.Instantiate(prefab, startPos, rot);
        go.GetComponent<KnifeProjectile>()?.Initialize(
            owner.GetInstanceID(), data, speed, distance, rotationSpeed
        );
    }
}
