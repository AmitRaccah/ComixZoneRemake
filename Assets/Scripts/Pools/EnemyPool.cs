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
        [Tooltip("A unique identifier for this spawn point.")]
        public string id;
        [Tooltip("The Transform where the enemy will be spawned.")]
        public Transform spawnPoint;
    }

    [Header("Spawn Configuration")]
    [Tooltip("A list of all possible spawn assignments in the level.")]
    [SerializeField] private SpawnAssignment[] spawnAssignments;

    [Header("Scene Enemies")]
    [Tooltip("Drag all enemy GameObjects from the Hierarchy that this pool should manage.")]
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

    public void StartEnemySequence(EnemyPoolMember enemy, string initialAssignmentId, float delay)
    {
        if (enemy == null || !enemiesInScene.Contains(enemy))
        {
            Debug.LogError($"Attempted to start a sequence for an enemy '{enemy?.name}' that is not registered in the EnemyPool's 'enemiesInScene' list. Make sure you dragged the enemy from the Hierarchy, not the Project folder.", this);
            return;
        }

        var sequence = enemy.GetComponent<EnemySpawnSequence>();
        if (sequence == null)
        {
            Debug.LogError($"The enemy '{enemy.name}' is missing the EnemySpawnSequence component.", enemy);
            return;
        }

        sequence.BeginSequence(initialAssignmentId, delay);
    }

    internal void Spawn(EnemyPoolMember member, string assignmentId)
    {
        if (!assignmentLookup.TryGetValue(assignmentId, out Transform spawnPoint))
        {
            Debug.LogError($"Spawn Assignment with ID '{assignmentId}' was not found in the EnemyPool.", this);
            return;
        }

        member.PrepareForSpawn(spawnPoint, assignmentId);
    }

    internal void ReturnToPool(EnemyPoolMember member)
    {
        if (member != null)
        {
            member.ReturnToPool();
        }
    }

    public void ScheduleSpawn(EnemyPoolMember member, string assignmentId, float delay)
    {
        if (member == null) return;

        if (scheduledSpawns.TryGetValue(member, out Coroutine existingCoroutine) && existingCoroutine != null)
        {
            StopCoroutine(existingCoroutine);
        }

        Coroutine newCoroutine = StartCoroutine(SpawnRoutine(member, assignmentId, delay));
        scheduledSpawns[member] = newCoroutine;
    }

    private IEnumerator SpawnRoutine(EnemyPoolMember member, string assignmentId, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        if (member != null)
        {
            Spawn(member, assignmentId);
            scheduledSpawns.Remove(member); 
        }
    }
}