using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonController))]
public class MovementLock : MonoBehaviour
{
    private bool locked;
    private ThirdPersonController ctrl;
    private int myId;

    private void Awake()
    {
     //   ctrl = GetComponent<ThirdPersonController>();
        myId = gameObject.GetInstanceID();
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<AttackStartedEvent>(OnAttackStarted);
        CombatBus.Subscribe<AttackEndedEvent>(OnAttackEnded);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<AttackStartedEvent>(OnAttackStarted);
        CombatBus.Unsubscribe<AttackEndedEvent>(OnAttackEnded);
    }

    private void OnAttackStarted(AttackStartedEvent e)
    {
        if (e.attackerId == myId)
            locked = true;
    }

    private void OnAttackEnded(AttackEndedEvent e)
    {
        if (e.attackerId == myId)
            locked = false;
    }

    public bool IsLocked => locked;
}
