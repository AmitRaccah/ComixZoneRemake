using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CanAttackAnimatorBridge : MonoBehaviour
{
    [SerializeField] private string canAttackBool = "CanAttack";
    private Animator anim; private int myId;
    void Awake() { anim = GetComponent<Animator>(); myId = gameObject.GetInstanceID(); }
    void OnEnable() => CombatBus.Subscribe<StunChangedEvent>(OnStun);
    void OnDisable() => CombatBus.Unsubscribe<StunChangedEvent>(OnStun);
    private void OnStun(StunChangedEvent e)
    {
        if (e.entityId != myId) return;
        anim.SetBool(canAttackBool, !e.isStunned);
    }
}
