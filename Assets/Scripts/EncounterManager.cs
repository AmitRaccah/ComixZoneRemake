using UnityEngine;
using System.Collections.Generic;

public class EncounterManager : MonoBehaviour
{
    [System.Serializable]
    public class Encounter
    {
        [Tooltip("A unique name for this encounter, e.g., 'FirstCorridor' or 'BossRoom'.")]
        public string encounterId;

        [Tooltip("How many enemy kills are required to complete this encounter.")]
        public int killsRequired = 1;

        [Tooltip("GameObjects (gates, arrows, etc.) to enable once the encounter is completed.")]
        public GameObject[] objectsToEnable;

        [HideInInspector] public int currentKills = 0;
        [HideInInspector] public bool isCompleted = false;
    }

    [Tooltip("List of all the encounters in the level.")]
    [SerializeField] private Encounter[] encounters;

    private Dictionary<string, Encounter> encounterLookup = new Dictionary<string, Encounter>();

    private void Awake()
    {
        foreach (var encounter in encounters)
        {
            if (encounter != null && !string.IsNullOrEmpty(encounter.encounterId))
            {
                encounterLookup[encounter.encounterId] = encounter;
                SetObjectsActive(encounter, false);
            }
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
        if (encounterLookup.TryGetValue(e.encounterId, out Encounter encounter))
        {
            if (encounter.isCompleted) return;

            encounter.currentKills++;
            Debug.Log($"Kill registered for encounter '{e.encounterId}'. Total kills: {encounter.currentKills}/{encounter.killsRequired}");

            if (encounter.currentKills >= encounter.killsRequired)
            {
                CompleteEncounter(encounter);
            }
        }
    }

    private void CompleteEncounter(Encounter encounter)
    {
        Debug.Log($"Encounter '{encounter.encounterId}' completed!");
        encounter.isCompleted = true;
        SetObjectsActive(encounter, true);
    }

    private void SetObjectsActive(Encounter encounter, bool isActive)
    {
        if (encounter.objectsToEnable == null) return;
        foreach (var obj in encounter.objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(isActive);
            }
        }
    }
}