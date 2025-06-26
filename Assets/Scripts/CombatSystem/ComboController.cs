
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
        resetT = 0.20f;                     

        var s = curSeq.steps[idx];
        anim.Trigger(s.trigger);

        CombatBus.Publish(new AttackStartedEvent(gameObject.GetInstanceID()));
    }
    void Tick()
    {
        // ­­­­­­­­­­­­­­­­­­­­­­­­­ 1) אין קליפ פעיל – מחכים לגרייס
        if (step < 0)
        {
            resetT -= Time.deltaTime;
            if (resetT <= 0f)                       // לא לחצו → סיום קומבו
            {
                CombatBus.Publish(
                    new AttackEndedEvent(gameObject.GetInstanceID()));
                canChain = false;                   // מחכים ללחיצה חדשה
            }
            return;
        }

        // ­­­­­­­­­­­­­­­­­­­­­­­­­ 2) יש קליפ פעיל
        resetT -= Time.deltaTime;
        if (resetT <= 0f && !canChain)              // קליפ נגמר, לא שרשרו
        {
            CombatBus.Publish(
                new AttackEndedEvent(gameObject.GetInstanceID()));
            step = -1;
        }
    }

    public void EnableChain() => canChain = true;
    public void DisableChain() => canChain = false;

    // called by Animation Event at the end of each clip
    public void EndStep()
    {
        resetT = 0.05f;      // חלון קצר ללחיצה נוספת
        canChain = true;     // מותר להתחיל Punch-1 חדש מיד

        // אם זה היה הקליפ האחרון בשרשרת – שחרר תנועה עכשיו
        if (step == curSeq.steps.Length - 1)
            CombatBus.Publish(
                new AttackEndedEvent(gameObject.GetInstanceID()));

        step = -1;           // חזרה למצב Idle בקומבו-מכונה
    }

}
