
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

    const float EarlyWindow = 0.15f;  
    InputType? queuedInput = null;     
    float queuedTimer = 0f;            


    void Awake() => anim = GetComponent<AnimationDriver>();

    void Update()
    {
        List<FrameInput> buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count > 0)
        {
            queuedInput = buf[^1].inputType;
            queuedTimer = EarlyWindow;

            buf.RemoveAt(buf.Count - 1);
        }

        if (queuedTimer > 0f) queuedTimer -= Time.deltaTime;
        if (queuedTimer <= 0f) queuedInput = null;

        if (step == -1 && queuedInput.HasValue && TryBegin(queuedInput.Value))
        {
            queuedInput = null;                
            Tick();
            return;
        }

        if (step >= 0 && queuedInput.HasValue && canChain &&
            step + 1 < curSeq.steps.Length &&
            curSeq.steps[step + 1].input == queuedInput.Value)
        {
            StartStep(step + 1);
            queuedInput = null;               
        }

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
      //  CombatBus.Publish(new AttackStartedEvent(gameObject.GetInstanceID()));
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
  //      CombatBus.Publish(new AttackEndedEvent(gameObject.GetInstanceID()));

        resetT = 0.25f; 
        step = -1;     
        canChain = false; 
    }

}
