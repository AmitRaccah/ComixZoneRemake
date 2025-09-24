using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyMotor : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float approachSpeed = 2.2f;
    [SerializeField] private float retreatSpeed = 2.4f;
    [SerializeField] private float brake = 12f;

    private Rigidbody body;
    private Animator anim;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }

    public void MoveTowards(Transform target)
    {
        if (!target) return;
        float dx = target.position.x - transform.position.x;
        float sign = Mathf.Sign(dx);

        Vector3 v = body.linearVelocity;
        float targetV = sign * approachSpeed;
        v.x = Mathf.MoveTowards(v.x, targetV, brake * Time.deltaTime);
        v.z = 0f;
        body.linearVelocity = v;

        FaceRight(sign > 0f);
        anim.SetFloat("Speed", Mathf.Abs(v.x));
        anim.SetFloat("MotionSpeed", 1f);
    }

    public void RetreatFrom(Transform target)
    {
        if (!target) return;
        float dx = target.position.x - transform.position.x;
        float sign = Mathf.Sign(dx);

        Vector3 v = body.linearVelocity;
        float targetV = -sign * retreatSpeed;
        v.x = Mathf.MoveTowards(v.x, targetV, brake * Time.deltaTime);
        v.z = 0f;
        body.linearVelocity = v;

        // בנסיגה עדיין מסתכלים על השחקן
        FaceRight(sign > 0f);
        anim.SetFloat("Speed", Mathf.Abs(v.x));
        anim.SetFloat("MotionSpeed", 1f);
    }

    public void StopMove()
    {
        Vector3 v = body.linearVelocity;
        v.x = Mathf.MoveTowards(v.x, 0f, brake * Time.deltaTime);
        v.z = 0f;
        body.linearVelocity = v;

        anim.SetFloat("Speed", Mathf.Abs(v.x));
        anim.SetFloat("MotionSpeed", 1f);
    }

    public void FaceRight(bool right)
    {
        transform.rotation = Quaternion.Euler(0f, right ? 90f : -90f, 0f);
        anim.SetBool("Mirror", !right);
    }
}
