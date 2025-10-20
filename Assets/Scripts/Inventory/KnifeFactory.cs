using UnityEngine;

public static class KnifeFactory
{
    public static void Spawn(GameObject owner, string knifeId, Transform socket, AttackData data, float speed, float distance, Vector3 spinPerSecond)
    {
        if (!owner || string.IsNullOrEmpty(knifeId) || data == null || KnifePoolManager.Instance == null) return;

        Vector3 startPos = socket ? socket.position : owner.transform.position + owner.transform.forward * 0.5f + Vector3.up * 1f;
        startPos.z = owner.transform.position.z;

        Vector3 f = socket ? socket.forward : owner.transform.forward;
        float sign = Mathf.Sign(Vector3.Dot(f, Vector3.right));
        if (sign == 0f) sign = 1f;
        Vector3 dir = new Vector3(sign, 0f, 0f);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        KnifePoolManager.Instance.Spawn(knifeId, startPos, rot, owner.GetInstanceID(), data, speed, distance, spinPerSecond);
    }
}
