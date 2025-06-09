using UnityEngine;
using UnityEngine.Rendering;

public class EnemyHitReaction : MonoBehaviour
{

    Animator anim;

    private void OnEnable()
    {
        anim = GetComponent<Animator>();
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != gameObject.GetInstanceID())
        {
            return;
        }
        anim.SetTrigger("Hit");
    }
}