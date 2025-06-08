using UnityEngine;

public class CrouchAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    private void OnEnable()
    {
        Debug.Log("Crouch Listener Enabled");
        CoreBus.Subscribe<PlayerCrouchEvent>(OnCrouchEvent);
        CoreBus.Subscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<PlayerCrouchEvent>(OnCrouchEvent);
        CoreBus.Unsubscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
    }

    private void OnCrouchEvent(PlayerCrouchEvent evt)
    {
        animationDriver.SetBool("IsCrouching", true);
    }

    private void OnUncrouchEvent(PlayerUncrouchEvent e)
    {
        Debug.Log("► Un-crouch event received");
        animationDriver.SetBool("IsCrouching", false);
    }



}
