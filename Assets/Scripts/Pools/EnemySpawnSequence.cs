using UnityEngine;
using System.Collections;

public class EnemySpawnSequence : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        [Tooltip("The ID of the spawn assignment from the EnemyPool.")]
        public string assignmentId;

        [Tooltip("The ID of the encounter this stage belongs to. Kills will count towards this encounter.")]
        public string encounterId;

        [Tooltip("How many times the enemy will spawn at this stage.")]
        public int spawnCount = 1;

        [Tooltip("Delay in seconds before respawning at the same stage.")]
        public float respawnDelay = 2f;

        [Tooltip("Delay in seconds before transitioning to the next stage.")]
        public float transitionDelay = 1f;
    }

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
            if (stages[i] != null && stages[i].assignmentId == initialAssignmentId)
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

        ScheduleSpawn(stages[currentStageIndex], delay, true);
    }

    private void OnReturnedToPool()
    {
        if (currentStageIndex == -1) return;

        if (livesRemainingInStage > 0)
        {
            ScheduleSpawn(stages[currentStageIndex], stages[currentStageIndex].respawnDelay);
        }
        else
        {
            currentStageIndex++;
            if (currentStageIndex < stages.Length)
            {
                Stage nextStage = stages[currentStageIndex];
                livesRemainingInStage = nextStage.spawnCount;
                ScheduleSpawn(nextStage, nextStage.transitionDelay);
            }
            else
            {
                currentStageIndex = -1; 
            }
        }
    }

    private void ScheduleSpawn(Stage stage, float delay, bool isFirstSpawn = false)
    {
        if (!isFirstSpawn)
        {
            livesRemainingInStage--;
        }

        EnemyPool.Instance.ScheduleSpawn(poolMember, stage.assignmentId, stage.encounterId, delay);
    }
}