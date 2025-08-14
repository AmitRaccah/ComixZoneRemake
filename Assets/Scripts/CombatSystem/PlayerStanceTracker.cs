using UnityEngine;
using StarterAssets;

[RequireComponent(typeof(StarterAssetsInputs))]
[RequireComponent(typeof(CharacterController))]
public class PlayerStanceTracker : MonoBehaviour
{
    public static PlayerStance Current;

    private StarterAssetsInputs input;
    private CharacterController cc;

    private void Awake()
    {
        input = GetComponent<StarterAssetsInputs>();
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (input == null) return;               

        if (input.crouch) Current = PlayerStance.Crouching;
        else if (input.lookUp) Current = PlayerStance.LookingUp;
        else if (!cc.isGrounded) Current = PlayerStance.Airborne;
        else Current = PlayerStance.Standing;
    }
}
