using UnityEngine;

public static class KnifeFactory
{
    public static KnifeSpawnParams BuildParams(GameObject owner, Transform socket, string knifeId, AttackData data, float speed, float distance, float rotX, float rotZ)
    {
        Vector3 startPos = socket ? socket.position : owner.transform.position + owner.transform.forward * 0.5f + Vector3.up;
        startPos.z = owner.transform.position.z;
        Vector3 f = socket ? socket.forward : owner.transform.forward;
        float sign = Mathf.Sign(Vector3.Dot(f, Vector3.right));
        if (sign == 0f) sign = 1f;
        Vector3 dir = new Vector3(sign, 0f, 0f);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        return new KnifeSpawnParams
        {
            knifeId = knifeId,
            attackerId = owner.GetInstanceID(),
            attackData = data,
            speed = speed,
            distance = distance,
            rotationSpeedX = rotX,
            rotationSpeedZ = rotZ,
            startPos = startPos,
            startRot = rot
        };
    }

    public static void Spawn(KnifeSpawnParams p)
    {
        if (KnifePoolManager.Instance == null) return;
        KnifePoolManager.Instance.Spawn(p.knifeId, p);
    }

    public static void Spawn(GameObject owner, string knifeId, Transform socket, AttackData data, float speed, float distance, float rotX, float rotZ)
    {
        var p = BuildParams(owner, socket, knifeId, data, speed, distance, rotX, rotZ);
        Spawn(p);
    }

    public static void Spawn(GameObject owner, string knifeId, Transform socket, AttackData data, float speed, float distance, float rotX)
    {
        Spawn(owner, knifeId, socket, data, speed, distance, rotX, 0f);
    }
}
