using UnityEngine;
using StarterAssets; 

public class AirAttackLimiter : MonoBehaviour
{
    private ThirdPersonController controller;
    private bool hasAttackedInAir;

    private void Awake()
    {
        controller = GetComponent<ThirdPersonController>();
    }

    private void Update()
    {
        if (controller.Grounded)
        {
            hasAttackedInAir = false;
        }
    }

    public bool CanStartAirAttack()
    {
        if (controller.Grounded)
        {
            return true;                
        }

        if (hasAttackedInAir)
        {
            return false;              
        }

        hasAttackedInAir = true;       
        return true;
    }
}
