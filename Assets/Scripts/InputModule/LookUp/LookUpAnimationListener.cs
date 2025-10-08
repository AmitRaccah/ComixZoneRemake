using UnityEngine;

public class LookUpAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    MovementLock movementLock;

    void Awake() => movementLock = GetComponent<MovementLock>();

    void OnEnable()
    {
        CoreBus.Subscribe<PlayerLookUpEvent>(OnLookUpEvent);
        CoreBus.Subscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<PlayerLookUpEvent>(OnLookUpEvent);
        CoreBus.Unsubscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
    }

    void OnLookUpEvent(PlayerLookUpEvent evt)
    {
        animationDriver.SetBool("IsLookUp", true);

        if (movementLock) movementLock.SetExternalLock(true);
    }

    void OnUnLookUpEvent(PlayerUnLookUpEvent evt)
    {
        animationDriver.SetBool("IsLookUp", false);

        if (movementLock) movementLock.SetExternalLock(false);
    }
}
