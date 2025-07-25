using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerStanceTracker : MonoBehaviour
{
    public static PlayerStance Current;

    Animator anim;
    CharacterController cc;

    void Awake()
    {
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        var input = GetComponent<StarterAssetsInputs>();

        if (input.crouch)                    
            Current = PlayerStance.Crouching;
        else if (input.lookUp)              
            Current = PlayerStance.LookingUp;
        else if (!cc.isGrounded)
            Current = PlayerStance.Airborne;
        else
            Current = PlayerStance.Standing;
    }

}
