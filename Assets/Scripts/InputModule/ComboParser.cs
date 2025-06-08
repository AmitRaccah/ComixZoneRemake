using UnityEngine;
using System.Collections.Generic;

public class ComboParser : MonoBehaviour
{

    [SerializeField] private AnimationDriver animationDriver;

    [SerializeField] private int maxFramesBack = 12;

    [SerializeField] private AttackActivator attackActivator;


    private bool MatchSinglePunch(List<FrameInput> buffer)
    {
        if (buffer.Count == 0) return false;

        FrameInput last = buffer[buffer.Count - 1];

        if (last.inputType == InputType.Punch)
        {
            for (int i = buffer.Count - 2; i >= 0; i--)
            {
                if (last.frame - buffer[i].frame > maxFramesBack)
                    break;

                if (buffer[i].inputType != InputType.Punch)
                    return false;
            }
            return true;
        }
        return false;
    }

    private bool MatchCrouchHit(List<FrameInput> buffer)
    {
        if (buffer.Count < 2) return false; //MUST have 2 frames

        FrameInput last = buffer[buffer.Count - 1];
        if (last.inputType != InputType.Punch)
        {
            return false;
        }

        for (int i = buffer.Count - 2; i >= 0; i--)
        {
            if (last.frame - buffer[i].frame > maxFramesBack)
                break;

            if (buffer[i].inputType == InputType.Crouch)
                return true;
        }
        return false;
    }

    private void ClearLastInput()
    {
        List<FrameInput> buffer = InputBuffer.Instance.GetBuffer();
        buffer.RemoveAt(buffer.Count - 1);
    }

    // Update is called once per frame
    void Update()
    {
        List<FrameInput> buffer = InputBuffer.Instance.GetBuffer();

        if (MatchSinglePunch(buffer))
        {
          

            animationDriver.Trigger("Punch");
         //   attackActivator.ActivateAttack("LightPunch");

            ClearLastInput();
        }
        if (MatchCrouchHit(buffer))
        {
            animationDriver.Trigger("CrouchPunch");
            ClearLastInput();
        }

    }
}
