using UnityEngine;

public class LookUpAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    // we’ll grab MovementLock on the same GameObject
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

    /* ——— callbacks ——— */
    void OnLookUpEvent(PlayerLookUpEvent evt)
    {
        animationDriver.SetBool("IsLookUp", true);

        // lock horizontal movement while looking up
        if (movementLock) movementLock.SetExternalLock(true);
    }

    void OnUnLookUpEvent(PlayerUnLookUpEvent evt)
    {
        animationDriver.SetBool("IsLookUp", false);

        // release the lock
        if (movementLock) movementLock.SetExternalLock(false);
    }
}
