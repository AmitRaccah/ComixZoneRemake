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
        var buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count > 0)
        {
            queuedInput = buf[^1].inputType;
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

        if (!chained && queuedInput.HasValue && TryBegin(queuedInput.Value))
        {
            queuedInput = null;
            return;                     
        }

        if (step >= 0)
        {
            resetT -= Time.deltaTime;
            if (resetT <= 0f) step = -1;
        }
    }


    bool TryBegin(InputType firstInput)
    {
        AttackSequence best = null;

        foreach (var e in combos)
        {
            var seq = e.sequence;
            if (seq == null || seq.steps.Length == 0) continue;

            if (seq.steps[0].input != firstInput) continue;

            if (seq.steps.Length > 1 && queuedInput.HasValue &&
                seq.steps[1].input != queuedInput.Value)
                continue;

            if (best == null || seq.steps.Length > best.steps.Length)
                best = seq;
        }

        if (best != null)
        {
            curSeq = best;
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
