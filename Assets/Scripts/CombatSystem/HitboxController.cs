using UnityEngine;

public class HitboxController : MonoBehaviour
{
    AttackData data;
    Transform socket;
    float timer;
    bool armed;
    public event System.Action OnFirstHit;
    int attackerId;

    public void Init(AttackData d, Transform hand, int attackerId)
    {
        data = d;
        socket = hand;
        this.attackerId = attackerId;
        timer = d.activeTime;

        transform.localScale = Vector3.one * d.hitboxRadius;
        GetComponent<Collider>().enabled = false;
    }

    void LateUpdate()
    {
        if (socket == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = socket.TransformPoint(data.hitboxOffset);

        if (!armed)
        {
            GetComponent<Collider>().enabled = true;
            armed = true;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!armed || other.transform == socket) return;

        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;
        if (root == socket.root) return;

        HitResolve.PublishDamageAndFx(attackerId, data, other, transform.position, transform);


        OnFirstHit?.Invoke();
        Destroy(gameObject);
    }
}
