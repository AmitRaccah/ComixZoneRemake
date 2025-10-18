using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EncounterManager : MonoBehaviour
{
    [System.Serializable]
    public class HazardTrigger
    {
        public int onKills;
        public EnemyPoolMember hazard;
        public string assignmentId;
        public float delay;
        public HazardSide side;
        public float warningLeadTime = 0.5f;
    }

    [System.Serializable]
    public class EnemyTrigger
    {
        public int onKills;
        public EnemyPoolMember enemy;
        public string initialAssignmentId;
        public float initialDelay;
    }

    [System.Serializable]
    public class Encounter
    {
        public string encounterId;
        public int killsRequired = 1;
        public GameObject[] objectsToEnable;
        public HazardTrigger[] hazardTriggers;
        public EnemyTrigger[] enemyTriggers;
        [HideInInspector] public int currentKills = 0;
        [HideInInspector] public bool isCompleted = false;
    }

    [SerializeField] private Encounter[] encounters;
    readonly Dictionary<string, Encounter> encounterLookup = new();

    void Awake()
    {
        foreach (var enc in encounters)
        {
            if (enc == null || string.IsNullOrEmpty(enc.encounterId)) continue;
            encounterLookup[enc.encounterId] = enc;
            SetObjectsActive(enc, false);
        }
    }

    void OnEnable() { CoreBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated); }
    void OnDisable() { CoreBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated); }

    void OnEnemyDefeated(EnemyDefeatedEvent e)
    {
        if (!encounterLookup.TryGetValue(e.encounterId, out var enc) || enc.isCompleted) return;

        enc.currentKills++;
        TriggerHazards(enc, e.encounterId);
        TriggerEnemies(enc);

        if (enc.currentKills >= enc.killsRequired)
        {
            enc.isCompleted = true;
            SetObjectsActive(enc, true);
        }
    }

    void TriggerHazards(Encounter enc, string encounterId)
    {
        if (enc.hazardTriggers == null || EnemyPool.Instance == null) return;

        for (int i = 0; i < enc.hazardTriggers.Length; i++)
        {
            var t = enc.hazardTriggers[i];
            if (t == null || t.hazard == null) continue;
            if (t.onKills != enc.currentKills) continue;

            EnemyPool.Instance.ScheduleSpawn(t.hazard, t.assignmentId, encounterId, t.delay);

            if (HazardWarningUI.Instance)
                StartCoroutine(WarningRoutine(t));
        }
    }

    void TriggerEnemies(Encounter enc)
    {
        if (enc.enemyTriggers == null) return;
        var pool = EnemyPool.Instance;
        if (!pool) return;

        for (int i = 0; i < enc.enemyTriggers.Length; i++)
        {
            var t = enc.enemyTriggers[i];
            if (t == null || t.enemy == null) continue;
            if (t.onKills != enc.currentKills) continue;

            pool.StartEnemySequence(t.enemy, t.initialAssignmentId, t.initialDelay);
        }
    }

    IEnumerator WarningRoutine(HazardTrigger t)
    {
        float wait = Mathf.Max(0f, t.delay - Mathf.Max(0f, t.warningLeadTime));
        if (wait > 0f) yield return new WaitForSeconds(wait);
        HazardWarningUI.Instance.Ping(t.side);
    }

    void SetObjectsActive(Encounter enc, bool isActive)
    {
        if (enc.objectsToEnable == null) return;
        foreach (var go in enc.objectsToEnable) if (go) go.SetActive(isActive);
    }
}
