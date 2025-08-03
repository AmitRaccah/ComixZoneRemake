using UnityEngine;

[RequireComponent(typeof(Animator))]
public class HitReaction : MonoBehaviour
{
    [Header("Animator")]
    [SerializeField] string triggerName = "Hit";

    Animator anim;
    int myId, triggerHash;

    void Awake()
    {
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
        triggerHash = Animator.StringToHash(triggerName);
    }

    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;
        anim.SetTrigger(triggerHash);
    }
}
