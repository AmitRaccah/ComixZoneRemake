using UnityEngine;
using System.Collections;

public class EnemySpawnSequence : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        [Tooltip("The ID of the spawn assignment from the EnemyPool.")]
        public string assignmentId;
        [Tooltip("How many times the enemy will spawn at this stage.")]
        public int spawnCount = 1;
        [Tooltip("Delay in seconds before respawning at the same stage.")]
        public float respawnDelay = 2f;
        [Tooltip("Delay in seconds before transitioning to the next stage.")]
        public float transitionDelay = 1f;
    }

    [Tooltip("The sequence of stages this enemy will follow.")]
    [SerializeField] private Stage[] stages;

    private EnemyPoolMember poolMember;
    private int currentStageIndex = -1;
    private int livesRemainingInStage;

    private void Awake()
    {
        poolMember = GetComponent<EnemyPoolMember>();
    }

    public void BeginSequence(string initialAssignmentId, float delay)
    {
        int initialStageIndex = -1;
        for (int i = 0; i < stages.Length; i++)
        {
            if (stages[i].assignmentId == initialAssignmentId)
            {
                initialStageIndex = i;
                break;
            }
        }

        if (initialStageIndex == -1)
        {
            Debug.LogError($"Could not find an initial stage with ID '{initialAssignmentId}' for enemy {name}.", this);
            return;
        }

        currentStageIndex = initialStageIndex;
        livesRemainingInStage = stages[currentStageIndex].spawnCount;

        ScheduleSpawn(stages[currentStageIndex].assignmentId, delay);
    }

    private void OnReturnedToPool()
    {
        if (currentStageIndex == -1) return;

        if (livesRemainingInStage > 0)
        {
            Stage currentStage = stages[currentStageIndex];
            ScheduleSpawn(currentStage.assignmentId, currentStage.respawnDelay);
        }
        else
        {
            currentStageIndex++;
            if (currentStageIndex < stages.Length)
            {
                Stage nextStage = stages[currentStageIndex];
                livesRemainingInStage = nextStage.spawnCount;
                ScheduleSpawn(nextStage.assignmentId, nextStage.transitionDelay);
            }
            else
            {
                currentStageIndex = -1; // Sequence finished
            }
        }
    }

    private void ScheduleSpawn(string assignmentId, float delay)
    {
        // The enemy is inactive, so it asks the active Pool Manager to run the coroutine for it.
        livesRemainingInStage--;
        EnemyPool.Instance.ScheduleSpawn(poolMember, assignmentId, delay);
    }
}