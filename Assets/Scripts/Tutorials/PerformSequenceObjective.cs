using System.Collections.Generic;
using UnityEngine;

public class PerformSequencesObjective : TutorialObjective
{
    [SerializeField] private AttackSequence[] sequences;
    [SerializeField] private GameObject playerActor;
    [SerializeField] private GameObject targetActor;

    private const float STEP_TIMEOUT = 1.2f;       // חלון בין צעדים
    private const float HIT_CONFIRM_WINDOW = 0.6f; // חלון לפגיעה אחרי הצעד האחרון

    private int targetId;

    private class Tracker
    {
        public AttackSequence seq;
        public int stepIndex;
        public float lastStepTime;
        public bool awaitingHit;
        public float awaitHitUntil;
        public bool completed;
    }

    private readonly List<Tracker> trackers = new();
    private int completedCount;

    // במקום לסמוך על גידול בכמות בפריימבאפר, נאפשר קלט אחד לפריים:
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
        Debug.Log($"[Tutor] Combos Begin. TargetId={targetId}, Trackers={trackers.Count}");
    }

    protected override void OnEnd()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
        Debug.Log("[Tutor] Combos End.");
    }

    private void CacheIds()
    {
        targetId = targetActor ? targetActor.GetInstanceID() : 0;
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
                    completed = false
                });
            }
        }

        var names = new System.Text.StringBuilder();
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

        // timeouts
        for (int i = 0; i < trackers.Count; i++)
        {
            var t = trackers[i];
            if (t.completed) continue;

            if (t.stepIndex > 0 && now - t.lastStepTime > STEP_TIMEOUT)
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

        // קלט אחד לפריים (כדי לא לעבד אותו דבר שוב באותו פריים)
        if (Time.frameCount == lastProcessedFrame) return;
        lastProcessedFrame = Time.frameCount;

        var buf = InputBuffer.Instance.GetBuffer();
        if (buf.Count == 0) return;

        var last = buf[buf.Count - 1]; // לא מוחקים מה-buffer

        // 1) בוחרים את הקומבו "הכי טוב": מתקדם קודם, ואז יותר צעדים שנותרו
        int best = -1;
        int bestScore = int.MinValue;

        // 2) במקביל מזהים קומבואים חד-שלביים שמתאימים (כדי שלא ייגנבו)
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
                if (singleStepMatches == null) singleStepMatches = new List<int>(2);
                singleStepMatches.Add(i);
            }
        }

        // קודם: לקדם את המועמד הטוב ביותר
        if (best >= 0)
        {
            var t = trackers[best];
            var steps = t.seq.steps;
            t.stepIndex++;
            t.lastStepTime = now;

            Debug.Log($"[Tutor] Input Accepted: {t.seq.name} step {t.stepIndex}/{steps.Length} key={last.inputType}");

            if (t.stepIndex >= steps.Length)
            {
                t.awaitingHit = true;
                t.awaitHitUntil = now + HIT_CONFIRM_WINDOW;
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

        // ואז: לאשר במקביל קומבואים חד-שלביים שתואמים
        if (singleStepMatches != null)
        {
            for (int k = 0; k < singleStepMatches.Count; k++)
            {
                int idx = singleStepMatches[k];
                if (idx == best) continue; // כבר טופל

                var t = trackers[idx];
                if (t.completed || t.awaitingHit) continue;
                var steps = t.seq.steps;

                t.stepIndex = steps.Length;
                t.lastStepTime = now;
                t.awaitingHit = true;
                t.awaitHitUntil = now + HIT_CONFIRM_WINDOW;

                Debug.Log($"[Tutor] Single-step match: {t.seq.name} → Await Hit until {t.awaitHitUntil:F2}");
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
            }
            break;
        }
    }
}
