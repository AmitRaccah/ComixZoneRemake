using UnityEngine;
using System.Collections.Generic;

public class EnemyCombatState : MonoBehaviour
{
  public bool GotHit { get; private set; }
  public bool HitRecently { get; private set; }
  public bool IsBeingSpammed { get; private set; }
  
  [Header ("Setting")]
  [SerializeField] private float hitRecentlyDuration = 1f;    // caculate how long count as "Recently hit"
  [SerializeField] private float spamWindow = 2f;             // time window to count hits
  [SerializeField] private float spamThreshold = 3;           // how many hits counts as spam
  [SerializeField] private float gotHitPulseDuration = 0.1f; // for how long "GotHit" stay "True"
  
  private float lastHitTime = -999f;
  private float gotHitUntil = -999f;
  private readonly Queue<float> _hitTimes = new Queue<float>();

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
   
   if (GotHit && now >= gotHitUntil)
     GotHit = false;
   
   while (_hitTimes.Count > 0 && _hitTimes.Peek() < now - spamWindow)
     _hitTimes.Dequeue();
   if (_hitTimes.Count < spamThreshold)
     IsBeingSpammed = false;
  }
}
