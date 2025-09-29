using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)]
public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class PrewarmEntry
    {
        [Tooltip("The enemy prefab to prewarm into the pool.")]
        public GameObject prefab;

        [Tooltip("How many instances of this prefab should be pre-created.")]
        public int count = 1;
    }

    [System.Serializable]
    public class SpawnAssignment
    {
        [Tooltip("Optional identifier used to request this spawn slot by name.")]
        public string id;

        [Tooltip("The enemy prefab that should be spawned for this slot.")]
        public GameObject prefab;

        [Tooltip("Where the enemy should appear when spawned from this slot.")]
        public Transform spawnPoint;

        [Tooltip("Optional parent override applied when the enemy is activated.")]
        public Transform parentOverride;

        [Tooltip("How many instances of this prefab should be kept ready for this slot.")]
        public int prewarmCount = 1;

        [Tooltip("Automatically spawn this slot on Start().")]
        public bool spawnOnStart = false;

        [Tooltip("Delay before automatically spawning this slot.")]
        public float initialSpawnDelay = 0f;
    }

    public static EnemyPool Instance { get; private set; }

    [SerializeField]
    private PrewarmEntry[] prewarmEntries;

    [SerializeField]
    [Tooltip("Configure spawn slots that bind enemy prefabs to spawn points.")]
    private SpawnAssignment[] spawnAssignments;

    [SerializeField]
    [Tooltip("Optional container used to hold inactive pooled enemies.")]
    private Transform inactiveContainer;

    private readonly Dictionary<GameObject, Queue<EnemyPoolMember>> pools = new();
    private readonly Dictionary<string, SpawnAssignment> assignmentLookup = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate EnemyPool detected. Destroying the new instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inactiveContainer == null)
        {
            inactiveContainer = transform;
        }

        PrewarmConfiguredEntries();
        PrepareSpawnAssignments();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        ActivateAutoSpawnAssignments();
    }

    private void PrewarmConfiguredEntries()
    {
        if (prewarmEntries == null) return;

        for (int i = 0; i < prewarmEntries.Length; i++)
        {
            PrewarmEntry entry = prewarmEntries[i];
            if (entry == null || entry.prefab == null) continue;

            EnsurePool(entry.prefab);
            Queue<EnemyPoolMember> queue = pools[entry.prefab];
            int toCreate = Mathf.Max(0, entry.count - queue.Count);

            for (int j = 0; j < toCreate; j++)
            {
                EnemyPoolMember member = CreateInstance(entry.prefab);
                queue.Enqueue(member);
            }
        }
    }

    private void PrepareSpawnAssignments()
    {
        assignmentLookup.Clear();
        if (spawnAssignments == null) return;

        Dictionary<GameObject, int> requiredCounts = new();

        for (int i = 0; i < spawnAssignments.Length; i++)
        {
            SpawnAssignment assignment = spawnAssignments[i];
            if (assignment == null || assignment.prefab == null) continue;

            if (!string.IsNullOrEmpty(assignment.id))
            {
                if (assignmentLookup.ContainsKey(assignment.id))
                {
                    Debug.LogWarning($"Duplicate enemy pool assignment id '{assignment.id}' detected. Overwriting the previous entry.");
                }

                assignmentLookup[assignment.id] = assignment;
            }

            int targetCount = Mathf.Max(1, assignment.prewarmCount);
            if (requiredCounts.TryGetValue(assignment.prefab, out int existing))
            {
                requiredCounts[assignment.prefab] = existing + targetCount;
            }
            else
            {
                requiredCounts[assignment.prefab] = targetCount;
            }
        }

        foreach (KeyValuePair<GameObject, int> kvp in requiredCounts)
        {
            Prewarm(kvp.Key, kvp.Value);
        }
    }

    private void ActivateAutoSpawnAssignments()
    {
        if (spawnAssignments == null) return;

        for (int i = 0; i < spawnAssignments.Length; i++)
        {
            SpawnAssignment assignment = spawnAssignments[i];
            if (assignment == null || !assignment.spawnOnStart) continue;

            StartCoroutine(SpawnAssignmentAfterDelay(assignment));
        }
    }

    private IEnumerator SpawnAssignmentAfterDelay(SpawnAssignment assignment)
    {
        if (assignment == null) yield break;

        if (assignment.initialSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(assignment.initialSpawnDelay);
        }

        SpawnUsingAssignment(assignment);
    }
    private IEnumerator SpawnAssignmentAfterDelay(SpawnAssignment assignment, float delay)
    {
        if (assignment == null) yield break;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        SpawnUsingAssignment(assignment);
    }

    private void EnsurePool(GameObject prefab)
    {
        if (prefab == null) return;
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<EnemyPoolMember>();
        }
    }

    private EnemyPoolMember CreateInstance(GameObject prefab)
    {
        GameObject go = Instantiate(prefab, inactiveContainer);
        go.SetActive(false);

        EnemyPoolMember member = go.GetComponent<EnemyPoolMember>();
        if (member == null)
        {
            member = go.AddComponent<EnemyPoolMember>();
        }

        member.Initialize(this, prefab);
        return member;
    }

    private EnemyPoolMember GetOrCreate(GameObject prefab)
    {
        EnsurePool(prefab);

        Queue<EnemyPoolMember> queue = pools[prefab];
        if (queue.Count > 0)
        {
            return queue.Dequeue();
        }

        return CreateInstance(prefab);
    }

    public GameObject SpawnEnemy(GameObject prefab, Transform spawnPoint)
    {
        return SpawnEnemy(prefab, spawnPoint, null);
    }

    public GameObject SpawnEnemy(GameObject prefab, Transform spawnPoint, Transform parentOverride)
    {
        if (prefab == null)
        {
            Debug.LogWarning("EnemyPool.SpawnEnemy called with a null prefab.");
            return null;
        }

        EnemyPoolMember member = GetOrCreate(prefab);
        member.PrepareForSpawn(parentOverride, spawnPoint, null);
        return member.gameObject;
    }

    public GameObject SpawnFromAssignment(int index)
    {
        if (spawnAssignments == null) return null;
        if (index < 0 || index >= spawnAssignments.Length) return null;

        SpawnAssignment assignment = spawnAssignments[index];
        return SpawnUsingAssignment(assignment);
    }

    internal Coroutine ScheduleSpawnFromAssignment(string id, float delay)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (!assignmentLookup.TryGetValue(id, out SpawnAssignment assignment))
        {
            Debug.LogWarning($"EnemyPool could not schedule spawn. No assignment with id '{id}'.");
            return null;
        }

        return StartCoroutine(SpawnAssignmentAfterDelay(assignment, Mathf.Max(0f, delay)));
    }

    public GameObject SpawnFromAssignment(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (!assignmentLookup.TryGetValue(id, out SpawnAssignment assignment))
        {
            Debug.LogWarning($"EnemyPool could not find a spawn assignment with id '{id}'.");
            return null;
        }

        return SpawnUsingAssignment(assignment);
    }

    private GameObject SpawnUsingAssignment(SpawnAssignment assignment)
    {
        if (assignment == null)
        {
            Debug.LogWarning("Attempted to spawn using a null assignment.");
            return null;
        }

        if (assignment.prefab == null)
        {
            Debug.LogWarning("Spawn assignment is missing a prefab reference.");
            return null;
        }

        EnemyPoolMember member = GetOrCreate(assignment.prefab);
        member.PrepareForSpawn(assignment.parentOverride, assignment.spawnPoint, assignment.id);
        return member.gameObject;
    }

    internal void Release(EnemyPoolMember member, float delay)
    {
        if (member == null) return;

        if (delay > 0f)
        {
            StartCoroutine(ReleaseRoutine(member, delay));
        }
        else
        {
            ReleaseImmediate(member);
        }
    }

    private IEnumerator ReleaseRoutine(EnemyPoolMember member, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReleaseImmediate(member);
    }

    private void ReleaseImmediate(EnemyPoolMember member)
    {
        if (member == null) return;

        bool returned = member.ReturnToPool(inactiveContainer);
        EnsurePool(member.SourcePrefab);
        if (returned)
        {
            pools[member.SourcePrefab].Enqueue(member);
        }
    }

    public void ReturnEnemy(GameObject enemy, float delay = 0f)
    {
        if (enemy == null) return;

        EnemyPoolMember member = enemy.GetComponent<EnemyPoolMember>();
        if (member == null)
        {
            Destroy(enemy);
            return;
        }

        Release(member, delay);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0) return;

        EnsurePool(prefab);
        Queue<EnemyPoolMember> queue = pools[prefab];
        int toCreate = Mathf.Max(0, count - queue.Count);

        for (int i = 0; i < toCreate; i++)
        {
            EnemyPoolMember member = CreateInstance(prefab);
            queue.Enqueue(member);
        }
    }
}