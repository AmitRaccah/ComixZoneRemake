using System.Collections.Generic;
using UnityEngine;

public class PerformSequencesObjective : TutorialObjective
{
    [SerializeField] private AttackSequence[] sequences;
    [SerializeField] private Sprite[] comboBalloons;
    [SerializeField] private GameObject targetActor;

    private const float STEP_TIMEOUT = 1.0f;
    private const float HIT_CONFIRM_WINDOW = 0.4f;

    private int targetId;

    private class Tracker
    {
        public AttackSequence seq;
        public int stepIndex;
        public float lastStepTime;
        public bool awaitingHit;
        public float awaitHitUntil;
        public bool completed;
        public int originalIndex;
    }

    private readonly List<Tracker> trackers = new();
    private int completedCount;
    private int lastProcessedFrame = -1;

    protected override void OnConfigure(TutorialManager manager)
    {
        CacheIds();
    }

    protected override void OnReset()
    {
        CacheIds();
        ResetTrackers();
    }

    protected override void OnBegin()
    {
        CacheIds();
        ResetTrackers();
        CombatBus.Subscribe<DamageEvent>(OnDamage);
        lastProcessedFrame = -1;
        ShowNextBalloon();
    }

    protected override void OnEnd()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void CacheIds()
    {
        targetId = targetActor ? targetActor.GetInstanceID() : 0;
    }

    private void ResetTrackers()
    {
        trackers.Clear();
        completedCount = 0;

        if (sequences == null) return;
        for (int i = 0; i < sequences.Length; i++)
        {
            var s = sequences[i];
            if (s == null || s.steps == null || s.steps.Length == 0) continue;
            trackers.Add(new Tracker
            {
                seq = s,
                stepIndex = 0,
                lastStepTime = -999f,
                awaitingHit = false,
                awaitHitUntil = -1f,
                completed = false,
                originalIndex = i
            });
        }
    }

    void Update()
    {
        if (!IsActive) return;
        if (trackers.Count == 0) return;
        if (InputBuffer.Instance == null) return;

        float now = Time.time;

        for (int i = 0; i < trackers.Count; i++)
        {
            var t = trackers[i];
            if (t.completed) continue;

            if (t.stepIndex > 0 && now - t.lastStepTime > STEP_TIMEOUT)
            {
                t.stepIndex = 0;
                t.awaitingHit = false;
                t.awaitHitUntil = -1f;
            }

            if (t.awaitingHit && now > t.awaitHitUntil)
            {
                t.stepIndex = 0;
                t.awaitingHit = false;
                t.awaitHitUntil = -1f;
            }
        }

        if (Time.frameCount == lastProcessedFrame) return;
        lastProcessedFrame = Time.frameCount;

        var buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count == 0) return;

        var last = buf[buf.Count - 1];

        int best = -1;
        int bestScore = int.MinValue;
        var stanceNow = PlayerStanceTracker.Current;

        for (int i = 0; i < trackers.Count; i++)
        {
            var t = trackers[i];
            if (t.completed || t.awaitingHit) continue;

            var steps = t.seq.steps;
            if (t.stepIndex >= steps.Length) continue;

            var req = steps[t.stepIndex];
            if (last.inputType != req.input) continue;
            if (req.stance != PlayerStance.Any && req.stance != stanceNow) continue;

            int remaining = steps.Length - t.stepIndex;
            int score = (t.stepIndex > 0 ? 1000 : 0) + remaining;
            if (score > bestScore) { bestScore = score; best = i; }
        }

        if (best >= 0)
        {
            var t = trackers[best];
            var steps = t.seq.steps;
            buf.RemoveAt(buf.Count - 1);
            t.stepIndex++;
            t.lastStepTime = now;

            if (t.stepIndex >= steps.Length)
            {
                t.awaitingHit = true;
                t.awaitHitUntil = now + HIT_CONFIRM_WINDOW;
            }
            else
            {
                t.awaitingHit = false;
                t.awaitHitUntil = -1f;
            }
        }
    }

    private void OnDamage(DamageEvent e)
    {
        if (!IsActive) return;
        if (targetId != 0 && e.targetId != targetId) return;
        if (e.isBlocked) return;

        float now = Time.time;

        for (int i = 0; i < trackers.Count; i++)
        {
            var t = trackers[i];
            if (t.completed) continue;
            if (!t.awaitingHit) continue;
            if (now > t.awaitHitUntil) continue;

            t.awaitingHit = false;
            t.awaitHitUntil = -1f;

            if (t.seq != null && t.stepIndex >= t.seq.steps.Length)
            {
                t.completed = true;
                completedCount++;

                if (completedCount >= trackers.Count && trackers.Count > 0)
                {
                    Manager.HideBalloon();
                    CompleteObjective();
                }
                else
                {
                    ShowNextBalloon();
                }
            }
            break;
        }
    }

    private void ShowNextBalloon()
    {
        int idx = NextPendingOriginalIndex();
        Sprite s = GetBalloon(idx);
        Manager.ShowBalloon(s);
    }

    private int NextPendingOriginalIndex()
    {
        for (int i = 0; i < trackers.Count; i++)
            if (!trackers[i].completed) return trackers[i].originalIndex;
        return -1;
    }

    private Sprite GetBalloon(int originalIndex)
    {
        if (originalIndex < 0) return null;
        if (comboBalloons == null) return null;
        if (originalIndex >= comboBalloons.Length) return null;
        return comboBalloons[originalIndex];
    }
}
