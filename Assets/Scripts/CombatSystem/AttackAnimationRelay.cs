//using UnityEngine;

//public class AttackAnimationRelay : MonoBehaviour
//{
//    int myId;
//    bool started;          

//    void Awake()
//    {
//        myId = GetInstanceID();
//    }

//    public void OnAttackStart()
//    {
//        if (started) return;        
//        started = true;

//        Debug.Log("[Relay] AttackStarted " + myId);
//        CombatBus.Publish(new AttackStartedEvent(myId));
//    }

//    public void OnAttackEnd()
//    {
//        if (!started) return;       
//        started = false;

//        Debug.Log("[Relay] AttackEnded " + myId);
//        CombatBus.Publish(new AttackEndedEvent(myId));
//    }
//}
