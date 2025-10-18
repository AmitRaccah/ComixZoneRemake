using UnityEngine;

[DisallowMultipleComponent]
public class EnemyFacePlayer : MonoBehaviour
{
    [SerializeField] bool invert;
    [SerializeField] float deadZone = 0.02f;
    [SerializeField] float turnSpeed = 720f;

    Transform player;

    void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;
    }

    void LateUpdate()
    {
        if (!player) return;
        float dx = player.position.x - transform.position.x;
        if (Mathf.Abs(dx) <= deadZone) return;
        float yaw = Axis2D.YawForDx(dx, invert);
        Quaternion target = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }
}
