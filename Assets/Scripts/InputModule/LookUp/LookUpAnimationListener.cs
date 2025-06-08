using UnityEngine;

public class LookUpAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    private void OnEnable()
    {
        CoreBus.Subscribe<PlayerLookUpEvent>(OnLookUpEvent);
        CoreBus.Subscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
    }

private void OnDisable()
{
    CoreBus.Unsubscribe<PlayerLookUpEvent>(OnLookUpEvent);
    CoreBus.Unsubscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
}

private void OnLookUpEvent(PlayerLookUpEvent evt)
{
    animationDriver.SetBool("IsLookUp", true);
}

private void OnUnLookUpEvent(PlayerUnLookUpEvent e)
{
    animationDriver.SetBool("IsLookUp", false);
}


}
