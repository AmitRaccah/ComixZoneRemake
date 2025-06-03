using UnityEngine;

public class LookUpAnimationListener : MonoBehaviour
{
    [SerializeField] private AnimationDriver animationDriver;

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerLookUpEvent>(OnLookUpEvent);
        EventBus.Subscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
    }

private void OnDisable()
{
    EventBus.Unsubscribe<PlayerLookUpEvent>(OnLookUpEvent);
    EventBus.Unsubscribe<PlayerUnLookUpEvent>(OnUnLookUpEvent);
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
