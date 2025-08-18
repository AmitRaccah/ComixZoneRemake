using UnityEngine;

public class PlayerKnockbackHandler : MonoBehaviour
{
    private CharacterController cc;
    private int myId;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        myId = gameObject.GetInstanceID();
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<KnockbackEvent>(this.OnKnockback);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<KnockbackEvent>(this.OnKnockback);
    }

    private void OnKnockback(KnockbackEvent e)
    {
        if (e.targetId != myId)
        {
            return;
        }
        Transform attackerTransform;
        if (!AttackActivator.TransformsById.TryGetValue(e.attackerId, out attackerTransform))
        {
            return;
        }
        Vector3 dir = (transform.position - attackerTransform.position).normalized;
        dir.y = 0f;
        cc.Move(dir * e.force * 0.05f); 
    }
}