
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AnimationDriver))]
public class ComboController : MonoBehaviour
{
    [System.Serializable] public struct Entry { public AttackSequence sequence; }

    [SerializeField] private Entry[] combos;

    AnimationDriver anim;
    AttackSequence curSeq;
    int step = -1;
    bool canChain;
    float resetT;

    void Awake() => anim = GetComponent<AnimationDriver>();

    void Update()
    {
        List<FrameInput> buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count == 0) { Tick(); return; }

        InputType inp = buf[^1].inputType;
        buf.RemoveAt(buf.Count - 1);

        if (step == -1 && TryBegin(inp)) { Tick(); return; }

        if (canChain && step + 1 < curSeq.steps.Length &&
            curSeq.steps[step + 1].input == inp)
            StartStep(step + 1);

        Tick();
    }

    bool TryBegin(InputType inp)
    {
        foreach (var e in combos)
        {
            var seq = e.sequence;
            if (seq && seq.steps.Length > 0 && seq.steps[0].input == inp)
            {
                curSeq = seq;
                StartStep(0);
                return true;
            }
        }
        return false;
    }

    void StartStep(int idx)
    {
        step = idx;
        canChain = false;
        resetT = 0.4f;

        var s = curSeq.steps[idx];
        anim.Trigger(s.trigger);

        // immediately lock movement
        CombatBus.Publish(new AttackStartedEvent(gameObject.GetInstanceID()));
    }

    void Tick()
    {
        if (step < 0) return;

        resetT -= Time.deltaTime;
        if (resetT <= 0f && !canChain)
        {
            CombatBus.Publish(new AttackEndedEvent(gameObject.GetInstanceID()));
            step = -1;
        }
    }

    public void EnableChain() => canChain = true;
    public void DisableChain() => canChain = false;

    // called by Animation Event at the end of each clip
    public void EndStep()
    {
        if (step == curSeq.steps.Length - 1)      // last clip
            CombatBus.Publish(new AttackEndedEvent(gameObject.GetInstanceID()));

        resetT = 0.25f;
        step = -1;
    }
}
