using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] private SpawnAssignment[] assignments;

    private readonly Dictionary<string, Transform> idToPoint = new Dictionary<string, Transform>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EnemyPool: duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        idToPoint.Clear();
        if (assignments != null)
        {
            for (int i = 0; i < assignments.Length; i++)
            {
                var a = assignments[i];
                if (a == null || string.IsNullOrEmpty(a.id) || a.spawnPoint == null) continue;
                if (idToPoint.ContainsKey(a.id))
                {
                    Debug.LogWarning("EnemyPool: duplicate assignment id '" + a.id + "' – overriding.");
                }
                idToPoint[a.id] = a.spawnPoint;
            }
        }
    }

    internal void Return(EnemyPoolMember member, float delay)
    {
        if (member == null) return;
        if (delay > 0f)
        {
            StartCoroutine(ReturnAfterDelay(member, delay));
        }
        else
        {
            member.ReturnToPool();
        }
    }

    private IEnumerator ReturnAfterDelay(EnemyPoolMember member, float delay)
    {
        yield return new WaitForSeconds(delay);
        member.ReturnToPool();
    }

    public Coroutine ScheduleRespawnSpecific(EnemyPoolMember member, string assignmentId, float delay)
    {
        if (member == null) return null;
        if (string.IsNullOrEmpty(assignmentId)) return null;

        Transform point;
        if (!idToPoint.TryGetValue(assignmentId, out point))
        {
            Debug.LogWarning("EnemyPool: assignment id not found: " + assignmentId);
            return null;
        }

        return StartCoroutine(RespawnSpecificAfterDelay(member, point, Mathf.Max(0f, delay), assignmentId));
    }

    private IEnumerator RespawnSpecificAfterDelay(EnemyPoolMember member, Transform point, float delay, string assignmentId)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (member == null) yield break;
        if (!member.IsPooled) { Debug.LogWarning("EnemyPool: member is not pooled."); yield break; }
        if (member.IsActive) { Debug.LogWarning("EnemyPool: member still active."); yield break; }

        member.PrepareForSpawn(point, assignmentId);
    }
}
