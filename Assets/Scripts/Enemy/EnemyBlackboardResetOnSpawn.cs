using UnityEngine;
using Unity.Behavior;
using System.Collections;

[RequireComponent(typeof(BehaviorGraphAgent))]
[DefaultExecutionOrder(-100)] // שירוץ לפני שאר ה-AI בפריים הראשון
public class EnemyBlackboardResetOnSpawn : MonoBehaviour
{
    BehaviorGraphAgent agent;

    void Awake() { agent = GetComponent<BehaviorGraphAgent>(); }

    void OnEnable() { ResetBB(); }
    void OnSpawnedFromPool() { ResetBB(); }

    void ResetBB()
    {
        TrySet("IsRetreating", false);
        TrySet("IsBeingSpammed", false);
        TrySet("IsStunned", false);
        TrySet("IsKnockback", false);
        TrySet("CanSeePlayer", false);
        TrySet("IsInAttackRange", false);
        TrySet("Distance", 999f);

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) TrySet("PlayerTransform", p);

        TrySet("RetreatTarget", gameObject);

        StartCoroutine(EndOfFrameFix());
    }

    IEnumerator EndOfFrameFix()
    {
        yield return new WaitForEndOfFrame();
        TrySet("IsRetreating", false);
        var anim = GetComponent<Animator>();
        if (anim) anim.SetInteger(Animator.StringToHash("AI_RetreatSteps"), 0);
    }

    void TrySet<T>(string name, T value)
    {
        try { agent.SetVariableValue(name, value); } catch { }
    }
}
