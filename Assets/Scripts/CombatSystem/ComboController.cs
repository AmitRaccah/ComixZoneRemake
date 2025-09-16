using UnityEngine;
using System.Collections.Generic;
using StarterAssets;

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

    Animator animator;

    void Awake()
    {
        anim = GetComponent<AnimationDriver>();
        animator = GetComponent<Animator>();
    }

    bool TryBeginLoopable(InputType firstInput)
    {
        PlayerStance stanceNow = PlayerStanceTracker.Current;

        foreach (var entry in combos)
        {
            var seq = entry.sequence;
            if (seq == null || !seq.loopableDuringAttack) continue;

            var first = seq.steps[0];
            if (first.input != firstInput) continue;
            if (first.stance != PlayerStance.Any && first.stance != stanceNow) continue;

            curSeq = seq;
            StartStep(0);
            return true;                   
        }
        return false;
    }

    void Update()
    {
        ThirdPersonController c = GetComponent<ThirdPersonController>();
        if (c != null && c.IsTurning)
        {
            step = -1;
            queuedInput = null;
            if (InputBuffer.Instance != null) InputBuffer.Instance.GetBuffer().Clear();
            activator.EndHitbox();
            return;
        }

        HitStunController s = GetComponent<HitStunController>();
        if (s != null && s.IsStunned)
        {
            step = -1;
            queuedInput = null;
            if (InputBuffer.Instance != null) InputBuffer.Instance.GetBuffer().Clear();
            activator.EndHitbox();
            return;
        }


        if (InputBuffer.Instance == null)
        {
            return;
        }

        if (step == -1)
        {
            bool stillInAttack =
                animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack") ||
                (animator.IsInTransition(0) &&
                 animator.GetNextAnimatorStateInfo(0).IsTag("Attack"));

            if (stillInAttack)
            {
                List<FrameInput> loopBuf = InputBuffer.Instance.GetBuffer();
                if (loopBuf.Count > 0)
                {
                    FrameInput last = loopBuf[^1];
                    if (TryBeginLoopable(last.inputType))
                    {
                        loopBuf.RemoveAt(loopBuf.Count - 1);
                        return;
                    }
                }

                InputBuffer.Instance.GetBuffer().Clear();
                queuedInput = null;
                return;
            }
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
        AttackSequence bestSeq = null;
        int bestScore = -1;

        foreach (var entry in combos)
        {
            var seq = entry.sequence;
            if (seq == null || seq.steps.Length == 0) continue;

            var first = seq.steps[0];
            if (first.input != firstInput) continue;
            if (first.stance != PlayerStance.Any && first.stance != stanceNow) continue;

            int score = seq.steps.Length;
            if (seq.steps.Length > 1 && queuedInput.HasValue &&
                seq.steps[1].input == queuedInput.Value)
                score += 100;         

            if (score > bestScore)
            {
                bestScore = score;
                bestSeq = seq;
            }
        }

        if (bestSeq != null)
        {
            curSeq = bestSeq;
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
        if (curSeq == null) return;
        if (step == curSeq.steps.Length - 1)
            step = -1;
    }
}


