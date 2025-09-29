using System.Collections;
using UnityEngine;

[AddComponentMenu("Pooling/Enemy Spawn Sequence")]
[DisallowMultipleComponent]
public class EnemySpawnSequence : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        [Tooltip("Identifier of the EnemyPool spawn assignment used for this stage.")]
        public string assignmentId;

        [Tooltip("Total number of times the enemy should appear at this stage (including the initial spawn).")]
        public int spawnCount = 1;

        [Tooltip("Delay before respawning at the same stage after dying.")]
        public float respawnDelay = 0f;

        [Tooltip("Optional delay applied before moving to the next stage once this stage is completed.")]
        public float transitionDelay = 0f;
    }

    [SerializeField]
    [Tooltip("Ordered list describing how this enemy should respawn across different rooms.")]
    private Stage[] stages;

    private EnemyPoolMember poolMember;
    private int currentStageIndex = -1;
    private int remainingLivesInStage;
    private bool sequenceInitialized;
    private Coroutine scheduledSpawn;

    private void Awake()
    {
        poolMember = GetComponent<EnemyPoolMember>();
        if (poolMember == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnSequence)} requires an {nameof(EnemyPoolMember)} component on {name}.");
        }
    }

    private void OnSpawnedFromPool()
    {
        if (poolMember == null || stages == null || stages.Length == 0)
        {
            return;
        }

        string assignmentId = poolMember.LastAssignmentId;
        int stageIndex = FindStageIndex(assignmentId);

        if (stageIndex < 0)
        {
            if (!sequenceInitialized)
            {
                Debug.LogWarning($"{nameof(EnemySpawnSequence)} on {name} could not match assignment '{assignmentId}'.");
            }
            return;
        }

        if (!sequenceInitialized)
        {
            sequenceInitialized = true;
            currentStageIndex = stageIndex;
            Stage stage = stages[currentStageIndex];
            remainingLivesInStage = Mathf.Max(0, stage.spawnCount - 1);
        }
    }

    private void OnReturnedToPool()
    {
        if (!sequenceInitialized || stages == null || stages.Length == 0)
        {
            return;
        }

        Stage stage = GetCurrentStage();
        if (stage == null)
        {
            return;
        }

        if (remainingLivesInStage > 0)
        {
            remainingLivesInStage--;
            ScheduleSpawn(stage.assignmentId, stage.respawnDelay);
        }
        else
        {
            int nextIndex = currentStageIndex + 1;
            if (nextIndex >= stages.Length)
            {
                sequenceInitialized = false;
                currentStageIndex = -1;
                return;
            }

            currentStageIndex = nextIndex;
            Stage nextStage = stages[currentStageIndex];
            remainingLivesInStage = Mathf.Max(0, nextStage.spawnCount - 1);
            float delay = nextStage.transitionDelay > 0f ? nextStage.transitionDelay : nextStage.respawnDelay;
            ScheduleSpawn(nextStage.assignmentId, delay);
        }
    }

    private void ScheduleSpawn(string assignmentId, float delay)
    {
        if (string.IsNullOrEmpty(assignmentId))
        {
            Debug.LogWarning($"{nameof(EnemySpawnSequence)} on {name} attempted to schedule a spawn without an assignment id.");
            return;
        }

        if (!isActiveAndEnabled)
        {
            return;
        }

        CancelScheduledSpawn();

        if (EnemyPool.Instance == null)
        {
            Debug.LogWarning($"{nameof(EnemySpawnSequence)} on {name} could not find an active {nameof(EnemyPool)} instance.");
            return;
        }

        scheduledSpawn = EnemyPool.Instance.StartCoroutine(SpawnAfterDelay(assignmentId, Mathf.Max(0f, delay)));
    }

    private IEnumerator SpawnAfterDelay(string assignmentId, float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        if (EnemyPool.Instance != null)
        {
            EnemyPool.Instance.SpawnFromAssignment(assignmentId);
        }
        else
        {
            Debug.LogWarning($"{nameof(EnemySpawnSequence)} on {name} could not execute spawn because the {nameof(EnemyPool)} instance was missing.");
        }

        scheduledSpawn = null;
    }

    private int FindStageIndex(string assignmentId)
    {
        if (string.IsNullOrEmpty(assignmentId) || stages == null)
        {
            return -1;
        }

        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i] != null && stages[i].assignmentId == assignmentId)
            {
                return i;
            }
        }

        return -1;
    }

    private Stage GetCurrentStage()
    {
        if (currentStageIndex < 0 || stages == null || currentStageIndex >= stages.Length)
        {
            return null;
        }

        return stages[currentStageIndex];
    }

    private void CancelScheduledSpawn()
    {
        if (scheduledSpawn != null)
        {
            if (EnemyPool.Instance != null)
            {
                EnemyPool.Instance.StopCoroutine(scheduledSpawn);
            }

            scheduledSpawn = null;
        }
    }
}