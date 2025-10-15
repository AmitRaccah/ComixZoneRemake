using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatState : MonoBehaviour
{
    public bool GotHit { get; private set; }
    public bool HitRecently { get; private set; }
    public bool IsBeingSpammed { get; private set; }

    [SerializeField] private float hitRecentlyDuration = 1f;
    [SerializeField] private float spamWindow = 2f;
    [SerializeField] private float spamThreshold = 3;
    [SerializeField] private float gotHitPulseDuration = 0.1f;

    private float lastHitTime = -999f;
    private float gotHitUntil = -999f;
    private readonly Queue<float> _hitTimes = new Queue<float>();

    void OnEnable() { ResetState(); }
    void OnSpawnedFromPool() { ResetState(); }

    public void RegisterHit()
    {
        float now = Time.time;
        lastHitTime = now;

        GotHit = true;
        gotHitUntil = now + gotHitPulseDuration;

        _hitTimes.Enqueue(now);
        while (_hitTimes.Count > 0 && _hitTimes.Peek() < now - spamWindow)
            _hitTimes.Dequeue();

        IsBeingSpammed = _hitTimes.Count >= spamThreshold;
    }

    void Update()
    {
        float now = Time.time;
        HitRecently = (now - lastHitTime) <= hitRecentlyDuration;

        if (GotHit && now >= gotHitUntil) GotHit = false;

        while (_hitTimes.Count > 0 && _hitTimes.Peek() < now - spamWindow)
            _hitTimes.Dequeue();
        if (_hitTimes.Count < spamThreshold) IsBeingSpammed = false;
    }

    void ResetState()
    {
        GotHit = false;
        HitRecently = false;
        IsBeingSpammed = false;
        lastHitTime = -999f;
        gotHitUntil = -999f;
        _hitTimes.Clear();
    }
}
