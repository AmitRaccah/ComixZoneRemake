using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(ThirdPersonController))]
public class MovementLock : MonoBehaviour
{
    bool locked;
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

    void OnStart(AttackStartedEvent e) { if (e.attackerId == myId) locked = true; }
    void OnEnd(AttackEndedEvent e) { if (e.attackerId == myId) locked = false; }

    public bool IsLocked => locked;
}
