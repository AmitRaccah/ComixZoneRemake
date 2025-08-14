//using UnityEngine;

//public class EnemyHitReaction : MonoBehaviour
//{
//    Animator anim;

//    void Awake() => anim = GetComponent<Animator>();

//    void OnEnable()
//    {
//        CombatBus.Subscribe<DamageEvent>(OnDamage);
//    }

//    void OnDisable()
//    {
//        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
//    }

//    void OnDamage(DamageEvent e)
//    {
//        if (e.targetId != gameObject.GetInstanceID()) return;
//        anim.SetTrigger("Hit");
//    }
//}
