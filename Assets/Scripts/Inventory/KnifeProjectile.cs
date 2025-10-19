using UnityEngine;
using DG.Tweening;

public class KnifeProjectile : MonoBehaviour
{
    private KnifePoolMember member;
    private int attackerId;
    private AttackData attackData;
    private float speed;
    private float distance;
    private float rotationSpeed;
    private Collider knifeCollider;
    private Tween moveTween;
    private Tween rotateTween;
    private bool active;

    void Awake()
    {
        knifeCollider = GetComponent<Collider>();
        if (knifeCollider) knifeCollider.isTrigger = true;
    }

    public void Activate(KnifePoolMember m, int atkId, AttackData data, float spd, float dist, float rotSpeed)
    {
        member = m;
        attackerId = atkId;
        attackData = data;
        speed = spd;
        distance = dist;
        rotationSpeed = rotSpeed;
        active = true;

        float duration = distance / speed;
        moveTween = transform.DOMove(transform.position + transform.forward * distance, duration).SetEase(Ease.Linear).OnComplete(End);
        rotateTween = transform.DORotate(new Vector3(rotationSpeed, 0, 0), duration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Incremental);
    }

    public void Deactivate()
    {
        active = false;
        if (moveTween != null) { moveTween.Kill(); moveTween = null; }
        if (rotateTween != null) { rotateTween.Kill(); rotateTween = null; }
    }

    void End()
    {
        if (!active) return;
        active = false;
        if (KnifePoolManager.Instance != null) KnifePoolManager.Instance.Return(member);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (other.transform.root.gameObject.GetInstanceID() == attackerId) return;

        var health = other.GetComponentInParent<Health>();
        if (health)
        {
            Vector3 hitPos = other.ClosestPoint(transform.position);
            Transform targetRoot = other.transform.root;
            if (targetRoot) hitPos.z = targetRoot.position.z;
            HitResolve.PublishDamageAndFx(attackerId, attackData, other, hitPos, null);
        }

        End();
    }
}
