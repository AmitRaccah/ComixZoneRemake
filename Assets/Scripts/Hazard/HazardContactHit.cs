using UnityEngine;

public class HazardContactHit : MonoBehaviour
{
    [SerializeField] private AttackData attack;
    [SerializeField] private LayerMask targetLayers;

    private int attackerId;

    void OnEnable()
    {
        attackerId = gameObject.GetInstanceID();
        AttackActivator.TransformsById[attackerId] = transform;
    }

    void OnDisable()
    {
        AttackActivator.TransformsById.Remove(attackerId);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!attack) return;
        if (((1 << other.gameObject.layer) & targetLayers.value) == 0) return;

        Transform root = other.attachedRigidbody ? other.attachedRigidbody.transform : other.transform.root;
        if (!root || root == transform.root) return;

        HitResolve.PublishDamageAndFx(attackerId, attack, other, transform.position, transform);
    }
}
