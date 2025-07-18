using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AnimationDriver))]
public class ComboController : MonoBehaviour
{
    [System.Serializable] public struct Entry { public AttackSequence sequence; }
    [SerializeField] private Entry[] combos;

    [SerializeField] private AttackActivator activator;


    AnimationDriver anim;
    AttackSequence curSeq;

    bool stanceNowLogged = false;

    int step = -1;
    int windowStep = -2;
    float resetT = 0.4f;

    InputType? queuedInput = null;

    void Awake()
    {
        anim = GetComponent<AnimationDriver>();
    }

    void Update()
    {

        if (InputBuffer.Instance == null)
        {
            return;
        }

        List<FrameInput> buf = InputBuffer.Instance.GetBuffer();

        if (buf.Count > 0)
        {
            FrameInput last = buf[buf.Count - 1];
            queuedInput = last.inputType;
            buf.RemoveAt(buf.Count - 1);
        }

        bool chained = false;
        if (step >= 0 &&
            queuedInput.HasValue &&
            windowStep == step &&
            step + 1 < curSeq.steps.Length &&
            curSeq.steps[step + 1].input == queuedInput.Value)
        {
            StartStep(step + 1);
            queuedInput = null;
            chained = true;
        }
        if (!chained && queuedInput.HasValue)
        {
            if (step < 0 && TryBegin(queuedInput.Value))
            {
                queuedInput = null;
                return;
            }
            queuedInput = null;
        }


        if (step >= 0)
        {
            resetT -= Time.deltaTime;
            if (resetT <= 0f) step = -1;
        }
    }


    bool TryBegin(InputType firstInput)
    {
        PlayerStance stanceNow = PlayerStanceTracker.Current;
        Debug.Log($"FIRST input={firstInput}  stanceNow={stanceNow}");

        if (stanceNow == PlayerStance.Airborne)
        {
            AirAttackLimiter limiter = GetComponent<AirAttackLimiter>();
            if (limiter != null && !limiter.CanStartAirAttack())
            {
                return false; 
            }
        }


        for (int i = 0; i < combos.Length; i++)
        {
            var seq = combos[i].sequence;
            if (seq == null || seq.steps.Length == 0) continue;

            var first = seq.steps[0];
            if (first.input != firstInput) continue;
            if (first.stance != PlayerStance.Any &&
                first.stance != stanceNow) continue;

            if (seq.steps.Length > 1 && queuedInput.HasValue)
            {
                var second = seq.steps[1];
                if (second.input != queuedInput.Value) continue;
                if (second.stance != PlayerStance.Any &&
                    second.stance != stanceNow) continue;

            }

            curSeq = seq;
            StartStep(0);
            return true;


        }

        return false;
    }


    void StartStep(int idx)
    {
        step = idx;
        windowStep = -2;
        resetT = 0.4f;

        var stepData = curSeq.steps[idx].attack;
        Debug.Log($"[Combo] Step {idx}: attack = {stepData.attackName}");
        activator.SetCurrentAttack(stepData);


        anim.Trigger(curSeq.steps[idx].trigger);
    }

    public void EnableChain()
    {
        Debug.Log("EnableChain()");
        windowStep = step;
    }

    public void DisableChain()
    {
        if (windowStep == step) windowStep = -2;
    }

    public void EndStep()
    {
        if (step == curSeq.steps.Length - 1) step = -1;
    }
}
