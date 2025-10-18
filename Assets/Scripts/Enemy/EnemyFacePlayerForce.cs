using UnityEngine;

public class EnemyFacePlayerForce : MonoBehaviour
{
    public enum ForwardAxis { X, Z }

    [SerializeField] ForwardAxis forwardAxis = ForwardAxis.Z;
    [SerializeField] bool invert = false;
    [SerializeField] float deadZone = 0.02f;

    Transform player;

    void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void LateUpdate()
    {
        Face();
    }

    void OnAnimatorMove()
    {
        Face();
    }

    void Face()
    {
        if (!player) return;
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= deadZone) return;

        float yaw;
        if (forwardAxis == ForwardAxis.Z)
            yaw = dx >= 0f ? 90f : -90f;
        else
            yaw = dx >= 0f ? 0f : 180f;

        if (invert) yaw = (yaw + 180f) % 360f;

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}
