using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementLock : MonoBehaviour
{
    private Animator anim;
    private bool locked;

    public bool IsLocked
    {
        get { return locked; }
    }

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        bool inAttack = anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
        bool nextAttack = anim.IsInTransition(0) &&
                          anim.GetNextAnimatorStateInfo(0).IsTag("Attack");

        locked = inAttack || nextAttack;
    }
}
