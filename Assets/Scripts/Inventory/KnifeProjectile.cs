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
    private Vector3 hitPositionWorld;

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
        Health targetHealth = other.GetComponentInParent<Health>();
        if (targetHealth)
        {
            hitPositionWorld = other.ClosestPoint(transform.position); 

            if (attackData.additionalHitEffects != null)
            {
                foreach (var fx in attackData.additionalHitEffects)
                {
                    if (fx.prefab != null)
                    {
                        Vector3 pos = hitPositionWorld
                            + transform.up * fx.localOffset.y
                            + transform.forward * fx.localOffset.z
                            + transform.right * fx.localOffset.x;
                        Instantiate(fx.prefab, pos, Quaternion.identity);
                    }
                }
            }

            CombatBus.Publish(new DamageEvent
            {
                attackerId = attackerId,
                targetId = other.transform.root.gameObject.GetInstanceID(),
                amount = attackData.damage,
                knockback = 0f,
                type = DamageType.Punch,
                shakeAmplitude = 0.5f,
                freezeFrameDuration = 0.1f,
                attackData = attackData
            });
        }
        Destroy(gameObject);
    }
}