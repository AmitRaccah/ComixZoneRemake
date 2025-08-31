using UnityEngine;
using DG.Tweening;

public class KnifeProjectile : MonoBehaviour
{
    private int attackerId;
    private AttackData attackData;
    private float speed;
    private float distance;
    private float rotationSpeed;
    private Collider knifeCollider;

    public void Initialize(int atkId, AttackData data, float spd, float dist, float rotSpeed)
    {
        attackerId = atkId;
        attackData = data;
        speed = spd;
        distance = dist;
        rotationSpeed = rotSpeed;
        knifeCollider = GetComponent<Collider>();
        knifeCollider.isTrigger = true;

        float duration = distance / speed;
        transform.DOMove(transform.position + transform.forward * distance, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() => Destroy(gameObject));

        transform.DORotate(new Vector3(rotationSpeed, 0, 0), duration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Incremental);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.gameObject.GetInstanceID() == attackerId) return;

        if (other.GetComponentInParent<Health>())
        {
            HitResolve.PublishDamageAndFx(attackerId, attackData, other, transform.position, transform);

        }

        Destroy(gameObject);
    }
}
