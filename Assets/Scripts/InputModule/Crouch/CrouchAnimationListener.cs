using UnityEngine;

public class CrouchAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    private void OnEnable()
    {
        Debug.Log("Crouch Listener Enabled");
        EventBus.Subscribe<PlayerCrouchEvent>(OnCrouchEvent);
        EventBus.Subscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerCrouchEvent>(OnCrouchEvent);
        EventBus.Unsubscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
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
