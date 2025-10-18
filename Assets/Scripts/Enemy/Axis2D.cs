using UnityEngine;

public enum ForwardAxis { X, Z }

public static class Axis2D
{
    public static ForwardAxis Forward = ForwardAxis.Z;

    public static float YawForDx(float dx, bool invert)
    {
        float yaw = Forward == ForwardAxis.Z ? (dx >= 0f ? 90f : -90f) : (dx >= 0f ? 0f : 180f);
        if (invert) yaw = (yaw + 180f) % 360f;
        return yaw;
    }

    public static bool IsInFront(Transform t, Vector3 pos)
    {
        Vector3 d = pos - t.position; d.y = 0f;
        Vector3 f = t.forward; f.y = 0f;
        if (f.sqrMagnitude > 0f) f.Normalize();
        return Vector3.Dot(f, d) >= 0f;
    }

    public static Vector3 BackDir(Transform self, Transform player)
    {
        float sign = player && player.position.x >= self.position.x ? -1f : 1f;
        if (Forward == ForwardAxis.Z) return new Vector3(sign, 0f, 0f);
        return new Vector3(0f, 0f, -sign);
    }
}
