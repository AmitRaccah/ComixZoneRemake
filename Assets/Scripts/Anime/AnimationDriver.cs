using UnityEngine;

public class AnimationDriver : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public void Trigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public void SetBool(string parameter, bool value)
    {
        animator.SetBool(parameter, value);
    }



}
