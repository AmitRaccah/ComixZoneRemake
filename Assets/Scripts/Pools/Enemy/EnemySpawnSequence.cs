using UnityEngine;

public class EnemySpawnSequence : MonoBehaviour
{
    [System.Serializable]
    public class Stage
    {
        public string assignmentId;
        public string encounterId;
        public int spawnCount = 1;
        public float respawnDelay = 2f;
    }

    [SerializeField] private Stage[] stages;

    private EnemyPoolMember poolMember;
    private int currentStageIndex = -1;
    private int livesRemainingInStage;

    private void Awake()
    {
        poolMember = GetComponent<EnemyPoolMember>();
    }

    public void BeginSequence(string initialAssignmentId, float initialDelay)
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

        ScheduleSpawn(stages[currentStageIndex], initialDelay, true);
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
                ScheduleSpawn(nextStage, 0f);
            }
            else
            {
                currentStageIndex = -1;
            }
        }
    }

    private void ScheduleSpawn(Stage stage, float delay, bool isFirstSpawn = false)
    {
        if (!isFirstSpawn) livesRemainingInStage--;
        EnemyPool.Instance.ScheduleSpawn(poolMember, stage.assignmentId, stage.encounterId, delay);
    }
}
