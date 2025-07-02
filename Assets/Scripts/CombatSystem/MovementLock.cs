using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MovementLock : MonoBehaviour
{
    bool locked;
    Animator anim;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        locked = anim.GetBool("IsAttacking");
    }

    public bool IsLocked
    {
        get { return locked; }
    }
}
