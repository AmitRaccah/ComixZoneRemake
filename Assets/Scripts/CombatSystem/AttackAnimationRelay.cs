using UnityEngine;

public class AttackAnimationRelay : MonoBehaviour
{
    int myId;
    void Awake() => myId = gameObject.GetInstanceID();

    public void OnAttackEnd()
    {
        CombatBus.Publish(new AttackEndedEvent(myId));
    }

    public void OnAttackStart()
    {
        CombatBus.Publish(new AttackStartedEvent(gameObject.GetInstanceID()));
    }

}
