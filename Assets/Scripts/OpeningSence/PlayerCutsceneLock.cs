using UnityEngine;
using StarterAssets;

public class PlayerCutsceneLock : MonoBehaviour
{
    public ThirdPersonController controller;
    public MovementLock movementLock;
    public StarterAssetsInputs inputs;

    public void FullLockBeforeIntro()
    {
        if (controller != null) controller.enabled = false;
        if (movementLock != null) movementLock.SetExternalLock(true);
        ClearInputs();
    }

    public void IdleLockAfterIntro()
    {
        if (controller != null) controller.enabled = false;
        if (movementLock != null) movementLock.SetExternalLock(true);
        ClearInputs();
    }

    public void FinalUnlock()
    {
        if (controller != null) controller.enabled = true;
        if (movementLock != null) movementLock.SetExternalLock(false);
    }

    void ClearInputs()
    {
        if (inputs == null) return;
        inputs.move = Vector2.zero;
        inputs.look = Vector2.zero;
        inputs.jump = false;
        inputs.sprint = false;
        inputs.crouch = false;
        inputs.block = false;
        inputs.pickUp = false;
        inputs.lookUp = false;
        inputs.analogMovement = false;
    }
}
