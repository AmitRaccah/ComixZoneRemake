using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementLock : MonoBehaviour
{
    private Animator _anim;
    private bool _externalLock = false;  //For other scripts

    public bool IsLocked
    {
        get
        {
            bool inAttack = _anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack");
            bool nextAttack =
                _anim.IsInTransition(0) &&
                _anim.GetNextAnimatorStateInfo(0).IsTag("Attack");

            return _externalLock || inAttack || nextAttack;
        }
    }

    public void SetExternalLock(bool state)
    {
        _externalLock = state;
    }

    private void Awake()
    {
        _anim = GetComponent<Animator>();
    }
}
