using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-50)]
public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    [System.Serializable]
    public class SpawnAssignment
    {
        public string id;
        public Transform spawnPoint;
    }

    [SerializeField] private SpawnAssignment[] spawnAssignments;
    [SerializeField] private List<EnemyPoolMember> enemiesInScene;

    private readonly Dictionary<string, Transform> assignmentLookup = new Dictionary<string, Transform>();
    private readonly Dictionary<EnemyPoolMember, Coroutine> scheduledSpawns = new Dictionary<EnemyPoolMember, Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (var assignment in spawnAssignments)
        {
            if (assignment != null && !string.IsNullOrEmpty(assignment.id) && assignment.spawnPoint != null)
            {
                assignmentLookup[assignment.id] = assignment.spawnPoint;
            }
        }
    }

    public void StartEnemySequence(EnemyPoolMember enemy, string initialAssignmentId, float initialDelay)
    {
        if (enemy == null || !enemiesInScene.Contains(enemy))
        {
            Debug.LogError($"Attempted to start a sequence for an enemy '{enemy?.name}' that is not registered in the EnemyPool.", this);
            return;
        }
        var sequence = enemy.GetComponent<EnemySpawnSequence>();
        if (sequence == null)
        {
            Debug.LogError($"The enemy '{enemy.name}' is missing the EnemySpawnSequence component.", enemy);
            return;
        }
        sequence.BeginSequence(initialAssignmentId, initialDelay);
    }

    internal void Spawn(EnemyPoolMember member, string assignmentId, string encounterId)
    {
        if (!assignmentLookup.TryGetValue(assignmentId, out Transform spawnPoint))
        {
            Debug.LogError($"Spawn Assignment with ID '{assignmentId}' was not found.", this);
            return;
        }
        member.PrepareForSpawn(spawnPoint, assignmentId, encounterId);
    }

    internal void ReturnToPool(EnemyPoolMember member)
    {
        if (member != null)
        {
            member.ReturnToPool();
        }
    }

    public void ScheduleSpawn(EnemyPoolMember member, string assignmentId, string encounterId, float delay)
    {
        if (member == null) return;
        if (scheduledSpawns.TryGetValue(member, out Coroutine existingCoroutine) && existingCoroutine != null)
        {
            StopCoroutine(existingCoroutine);
        }
        Coroutine newCoroutine = StartCoroutine(SpawnRoutine(member, assignmentId, encounterId, delay));
        scheduledSpawns[member] = newCoroutine;
    }

    private IEnumerator SpawnRoutine(EnemyPoolMember member, string assignmentId, string encounterId, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        if (member != null)
        {
            Spawn(member, assignmentId, encounterId);
            if (scheduledSpawns.ContainsKey(member))
            {
                scheduledSpawns.Remove(member);
            }
        }
    }
}
