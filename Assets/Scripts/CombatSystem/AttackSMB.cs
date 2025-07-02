using UnityEngine;

public class AttackSMB : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        animator.SetBool("IsAttacking", true);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        animator.SetBool("IsAttacking", false);
    }
}
