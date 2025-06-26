using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonController))]
public class MovementLock : MonoBehaviour
{
    int lockCount = 0;        
    int myId;

    void Awake() => myId = gameObject.GetInstanceID();

    void OnEnable()
    {
        CombatBus.Subscribe<AttackStartedEvent>(OnStart);
        CombatBus.Subscribe<AttackEndedEvent>(OnEnd);
    }
    void OnDisable()
    {
        CombatBus.Unsubscribe<AttackStartedEvent>(OnStart);
        CombatBus.Unsubscribe<AttackEndedEvent>(OnEnd);
    }

    void OnStart(AttackStartedEvent e)
    {
        if (e.attackerId != myId) return;
        lockCount++;

        // אפס מהירות מידית
        var ctrl = GetComponent<CharacterController>();
        if (ctrl) ctrl.Move(Vector3.zero);
    }

    void OnEnd(AttackEndedEvent e)
    {
        if (e.attackerId != myId) return;
        lockCount = Mathf.Max(0, lockCount - 1);
    }

    public bool IsLocked => lockCount > 0;
}
