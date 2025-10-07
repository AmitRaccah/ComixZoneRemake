using UnityEngine;
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
    }

    [System.Serializable]
    public class Encounter
    {
        public string encounterId;
        public int killsRequired = 1;
        public GameObject[] objectsToEnable;
        public HazardTrigger[] hazardTriggers;

        [HideInInspector] public int currentKills = 0;
        [HideInInspector] public bool isCompleted = false;
    }

    [SerializeField] private Encounter[] encounters;

    private readonly Dictionary<string, Encounter> encounterLookup = new Dictionary<string, Encounter>();

    private void Awake()
    {
        for (int i = 0; i < encounters.Length; i++)
        {
            var enc = encounters[i];
            if (enc == null || string.IsNullOrEmpty(enc.encounterId)) continue;
            encounterLookup[enc.encounterId] = enc;
            SetObjectsActive(enc, false);
        }
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<EnemyDefeatedEvent>(OnEnemyDefeated);
    }

    private void OnEnemyDefeated(EnemyDefeatedEvent e)
    {
        if (!encounterLookup.TryGetValue(e.encounterId, out var encounter)) return;
        if (encounter.isCompleted) return;

        encounter.currentKills++;
        TriggerHazards(encounter, e.encounterId);

        if (encounter.currentKills >= encounter.killsRequired)
        {
            encounter.isCompleted = true;
            SetObjectsActive(encounter, true);
        }
    }

    private void TriggerHazards(Encounter encounter, string encounterId)
    {
        if (encounter.hazardTriggers == null || EnemyPool.Instance == null) return;

        for (int i = 0; i < encounter.hazardTriggers.Length; i++)
        {
            var t = encounter.hazardTriggers[i];
            if (t == null || t.hazard == null) continue;
            if (t.onKills == encounter.currentKills)
                EnemyPool.Instance.ScheduleSpawn(t.hazard, t.assignmentId, encounterId, t.delay);
        }
    }

    private void SetObjectsActive(Encounter encounter, bool isActive)
    {
        if (encounter.objectsToEnable == null) return;
        for (int i = 0; i < encounter.objectsToEnable.Length; i++)
        {
            var go = encounter.objectsToEnable[i];
            if (go) go.SetActive(isActive);
        }
    }
}
