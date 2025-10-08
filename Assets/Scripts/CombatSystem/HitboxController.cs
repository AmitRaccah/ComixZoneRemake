using UnityEngine;

public class HitboxController : MonoBehaviour
{
    AttackData data;
    Transform socket;
    float timer;
    bool armed;
    public event System.Action OnFirstHit;
    int attackerId;

    HitboxPool pool;
    Collider col;

    public void AssignPool(HitboxPool p) => pool = p;

    public void Init(AttackData d, Transform hand, int attacker)
    {
        if (!col) col = GetComponent<Collider>();

        data = d;
        socket = hand;
        attackerId = attacker;
        timer = d.activeTime;
        armed = false;

        transform.localScale = Vector3.one * d.hitboxRadius;
        if (col) col.enabled = false;
    }

    void LateUpdate()
    {
        if (!socket) { Despawn(); return; }

        transform.position = socket.TransformPoint(data.hitboxOffset);

        if (!armed)
        {
            if (col) col.enabled = true;
            armed = true;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f) Despawn();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!armed || other.transform == socket) return;

        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;
        if (root == socket.root) return;

        HitResolve.PublishDamageAndFx(attackerId, data, other, transform.position, transform);

        OnFirstHit?.Invoke();
        Despawn();
    }

    public void Despawn()
    {
        if (col) col.enabled = false;
        armed = false;
        socket = null;
        data = null;
        attackerId = 0;

        pool?.Release(this);
    }
}
