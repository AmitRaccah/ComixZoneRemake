using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class PerformSequencesObjective : TutorialObjective
{
    [Header("Sequences")]
    [SerializeField] private AttackSequence[] sequences;
    [SerializeField] private GameObject playerActor;
    [SerializeField] private GameObject targetActor;

    [Header("Timing")]
    [SerializeField] private float stepTimeout = 2.0f;      // פרק זמן בין צעדי קומבו
    [SerializeField] private float hitConfirmWindow = 0.6f; // חלון לפגיעה אחרי הצעד האחרון

    [Header("Balloons")]
    [SerializeField] private BalloonEntry[] balloons;

    [System.Serializable]
    private struct BalloonEntry
    {
        public AttackSequence sequence;
        public Sprite sprite;
    }

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
    private readonly Dictionary<AttackSequence, Sprite> balloonBySeq = new();

    private int completedCount;
    private int targetId;
    private int lastProcessedFrame = -1;

    protected override void OnConfigure(TutorialManager manager)
    {
        CacheIds();
        BuildBalloonMap();
    }

    protected override void OnReset()
    {
        CacheIds();
        BuildBalloonMap();
        ResetTrackers();
    }

    protected override void OnBegin()
    {
        CacheIds();
        BuildBalloonMap();
        ResetTrackers();
        CombatBus.Subscribe<DamageEvent>(OnDamage);
        lastProcessedFrame = -1;
        Debug.Log($"[Tutor] Combos Begin. TargetId={targetId}, Trackers={trackers.Count}");
        ShowNextBalloon();
    }

    protected override void OnEnd()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
        Manager?.HideBalloon();
        Debug.Log("[Tutor] Combos End.");
    }

    private void CacheIds()
    {
        targetId = targetActor ? targetActor.GetInstanceID() : 0;
    }

    private void BuildBalloonMap()
    {
        balloonBySeq.Clear();
        if (balloons != null)
        {
            for (int i = 0; i < balloons.Length; i++)
            {
                var b = balloons[i];
                if (b.sequence == null) continue;
                if (!balloonBySeq.ContainsKey(b.sequence))
                    balloonBySeq.Add(b.sequence, b.sprite);
            }
        }
        if (sequences != null)
        {
            for (int i = 0; i < sequences.Length; i++)
            {
                var s = sequences[i];
                if (s == null) continue;
                if (!balloonBySeq.TryGetValue(s, out var spr) || spr == null)
                    Debug.LogWarning($"[Tutor] No balloon sprite mapped for sequence '{s.name}' (index {i}).");
            }
        }
    }

    private void ResetTrackers()
    {
        trackers.Clear();
        completedCount = 0;

        if (sequences != null)
        {
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

        var names = new StringBuilder();
        for (int i = 0; i < trackers.Count; i++)
        {
            if (i > 0) names.Append(", ");
            names.Append(trackers[i].seq.name);
        }
        Debug.Log($"[Tutor] Trackers Ready: [{names}]");
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

            if (t.stepIndex > 0 && now - t.lastStepTime > stepTimeout)
            {
                Debug.Log($"[Tutor] Timeout Reset: {t.seq.name} at step {t.stepIndex}");
                t.stepIndex = 0;
                t.awaitingHit = false;
                t.awaitHitUntil = -1f;
            }

            if (t.awaitingHit && now > t.awaitHitUntil)
            {
                Debug.Log($"[Tutor] Hit Window Expired: {t.seq.name}");
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

        List<int> singleStepMatches = null;

        var stanceNowGlobal = PlayerStanceTracker.Current;

        for (int i = 0; i < trackers.Count; i++)
        {
            var t = trackers[i];
            if (t.completed || t.awaitingHit) continue;

            var steps = t.seq.steps;
            if (t.stepIndex >= steps.Length) continue;

            var req = steps[t.stepIndex];
            if (last.inputType != req.input) continue;

            if (req.stance != PlayerStance.Any && req.stance != stanceNowGlobal) continue;

            int remaining = steps.Length - t.stepIndex;
            int score = (t.stepIndex > 0 ? 1000 : 0) + remaining;
            if (score > bestScore) { bestScore = score; best = i; }

            if (steps.Length == 1 && t.stepIndex == 0)
            {
                singleStepMatches ??= new List<int>(2);
                singleStepMatches.Add(i);
            }
        }

        if (best >= 0)
        {
            var t = trackers[best];
            var steps = t.seq.steps;
            t.stepIndex++;
            t.lastStepTime = now;

            Debug.Log($"[Tutor] Input Accepted: {t.seq.name} step {t.stepIndex}/{steps.Length} key={last.inputType}");

            if (t.stepIndex == 1)
            {
                if (balloonBySeq.TryGetValue(t.seq, out var spr) && spr != null)
                {
                    Debug.Log($"[Tutor] Balloon swap on begin → {t.seq.name} ({spr.name})");
                    Manager?.ShowBalloon(spr);
                }
            }

            if (t.stepIndex >= steps.Length)
            {
                t.awaitingHit = true;
                t.awaitHitUntil = now + hitConfirmWindow;
                Debug.Log($"[Tutor] Await Hit: {t.seq.name} until {t.awaitHitUntil:F2}");
            }
            else
            {
                t.awaitingHit = false;
                t.awaitHitUntil = -1f;
            }
        }
        else
        {
            Debug.Log($"[Tutor] Input Ignored: {last.inputType}");
        }

        if (singleStepMatches != null)
        {
            for (int k = 0; k < singleStepMatches.Count; k++)
            {
                int idx = singleStepMatches[k];
                if (idx == best) continue;

                var t = trackers[idx];
                if (t.completed || t.awaitingHit) continue;
                var steps = t.seq.steps;

                t.stepIndex = steps.Length;
                t.lastStepTime = now;
                t.awaitingHit = true;
                t.awaitHitUntil = now + hitConfirmWindow;

                Debug.Log($"[Tutor] Single-step match: {t.seq.name} → Await Hit until {t.awaitHitUntil:F2}");

                if (balloonBySeq.TryGetValue(t.seq, out var spr) && spr != null)
                {
                    Debug.Log($"[Tutor] Balloon swap on begin(single) → {t.seq.name} ({spr.name})");
                    Manager?.ShowBalloon(spr);
                }
            }
        }
    }

    private void OnDamage(DamageEvent e)
    {
        if (!IsActive) return;

        Debug.Log($"[Tutor] Damage Event: targetId={e.targetId} time={Time.time:F2}");
        if (targetId != 0 && e.targetId != targetId) return;

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
                Debug.Log($"[Tutor] Sequence Completed: {t.seq.name} ({completedCount}/{trackers.Count})");

                if (completedCount >= trackers.Count && trackers.Count > 0)
                {
                    Debug.Log("[Tutor] All Sequences Completed");
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
        Tracker t = null;
        Sprite s = null;

        for (int i = 0; i < trackers.Count; i++)
        {
            if (trackers[i].completed) continue;
            if (balloonBySeq.TryGetValue(trackers[i].seq, out var spr) && spr != null)
            {
                t = trackers[i];
                s = spr;
                break;
            }
        }

        if (t == null)
        {
            Debug.Log("[Tutor] No pending sequence with a mapped balloon → Hide.");
            Manager?.HideBalloon();
            return;
        }

        Debug.Log($"[Tutor] Balloon → index={t.originalIndex} sprite={(s ? s.name : "null")}");
        Manager?.ShowBalloon(s);
    }
}
