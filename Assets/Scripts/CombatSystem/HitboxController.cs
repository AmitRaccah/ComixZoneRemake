using UnityEngine;

public class HitboxController : MonoBehaviour
{
    AttackData data;
    Transform socket;
    float timer;
    bool armed;

    public void Init(AttackData d, Transform hand)
    {
        data = d;
        socket = hand;
        timer = d.activeTime;

        transform.localScale = Vector3.one * d.hitboxRadius;
        GetComponent<Collider>().enabled = false;
    }

    void LateUpdate()
    {
        transform.position = socket.TransformPoint(data.hitboxOffset);

        if (!armed) { GetComponent<Collider>().enabled = true; armed = true; }

        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!armed || other.transform == socket) return;

        Transform root = other.attachedRigidbody ?
                         other.attachedRigidbody.transform :
                         other.transform.root;

        CombatBus.Publish(new DamageEvent
        {
            attackerId = socket.root.gameObject.GetInstanceID(),   
            targetId = root.gameObject.GetInstanceID(),
            amount = data.damage,
            knockback = data.knockback,
            type = data.damageType
        });
    }
}
