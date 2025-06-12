using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AnimationDriver))]
public class ComboController : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public AttackSequence sequence;     // גרור כל קומבו לכאן
    }
    [SerializeField] Entry[] combos;        // מערך ב-Inspector

    AnimationDriver anim;
    AttackSequence curSeq;
    int step = -1;
    bool canChain;
    float resetT;

    void Awake() => anim = GetComponent<AnimationDriver>();

    void Update()
    {
        var buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count == 0) { Tick(); return; }

        var inp = buf[^1].inputType;
        buf.RemoveAt(buf.Count - 1);

        if (step == -1 && TryBeginSequence(inp)) return;

        if (canChain && step + 1 < curSeq.steps.Length &&
            curSeq.steps[step + 1].input == inp)
            StartStep(step + 1);

        Tick();
    }

    bool TryBeginSequence(InputType inp)
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
        resetT = .4f;

        anim.Trigger(curSeq.steps[idx].trigger);
    }

    void Tick()
    {
        if (step < 0) return;
        resetT -= Time.deltaTime;
        if (resetT <= 0f && !canChain) step = -1;
    }

    public void EnableChain() => canChain = true;
    public void DisableChain() => canChain = false;
    public void EndStep()                      
    {
        resetT = .25f;
        CombatBus.Publish(new AttackEndedEvent(gameObject.GetInstanceID()));
        step = -1;                              
    }
}
