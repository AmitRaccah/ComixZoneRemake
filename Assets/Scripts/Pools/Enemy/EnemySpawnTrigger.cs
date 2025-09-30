using UnityEngine;
public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private EnemyPoolMember enemyToTrigger;
    [SerializeField] private string initialAssignmentId;
    [SerializeField] private float startDelay = 0f;
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered || !other.CompareTag("Player")) return;

        EnemyPool.Instance.StartEnemySequence(enemyToTrigger, initialAssignmentId, startDelay);
        hasTriggered = true;
        gameObject.SetActive(false);
    }
}