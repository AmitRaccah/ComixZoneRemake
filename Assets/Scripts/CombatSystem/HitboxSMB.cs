using UnityEngine;

public class HitboxSMB : StateMachineBehaviour
{
    AttackActivator GetActivator(Animator animator) =>
        animator.GetComponent<AttackActivator>();

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        GetActivator(animator).BeginHitbox();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        GetActivator(animator).EndHitbox();
    }
}
