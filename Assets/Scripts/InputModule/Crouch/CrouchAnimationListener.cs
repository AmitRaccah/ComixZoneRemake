using UnityEngine;

public class CrouchAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    MovementLock movementLock;

    void Awake() => movementLock = GetComponent<MovementLock>();

    void OnEnable()
    {
        CoreBus.Subscribe<PlayerCrouchEvent>(OnCrouchEvent);
        CoreBus.Subscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<PlayerCrouchEvent>(OnCrouchEvent);
        CoreBus.Unsubscribe<PlayerUncrouchEvent>(OnUncrouchEvent);
    }

    void OnCrouchEvent(PlayerCrouchEvent _)
    {
        animationDriver.SetBool("IsCrouching", true);
        if (movementLock) movementLock.SetExternalLock(true);  
    }

    void OnUncrouchEvent(PlayerUncrouchEvent _)
    {
        animationDriver.SetBool("IsCrouching", false);
        if (movementLock) movementLock.SetExternalLock(false);
    }
}
