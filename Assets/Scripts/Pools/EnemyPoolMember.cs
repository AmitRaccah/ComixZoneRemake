using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool owner;
    private bool isInitialized;
    private bool isActive;
    private Vector3 initialLocalScale;
    private Quaternion initialLocalRotation;
    private string currentAssignmentId;

    internal bool IsActive { get { return isActive; } }
    internal bool IsPooled { get { return owner != null; } }
    internal string LastAssignmentId { get { return currentAssignmentId; } }

    private void Awake()
    {
        owner = EnemyPool.Instance;
        isInitialized = true;
        initialLocalScale = transform.localScale;
        initialLocalRotation = transform.localRotation;
        isActive = gameObject.activeSelf;
        currentAssignmentId = null;
    }

    internal void PrepareForSpawn(Transform spawnPoint, string assignmentId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"{nameof(EnemyPoolMember)} on {name} was not initialized before spawn.");
        }
        if (isActive)
        {
            Debug.LogWarning($"{nameof(EnemyPoolMember)} on {name} requested to spawn while already active.");
            return;
        }

        if (spawnPoint != null)
        {
            transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        }

        transform.localScale = initialLocalScale;

        ResetDynamicState();
        gameObject.SetActive(true);

        isActive = true;
        currentAssignmentId = assignmentId;

        BroadcastMessage("OnSpawnedFromPool", SendMessageOptions.DontRequireReceiver);
    }

    internal bool ReturnToPool()
    {
        if (!isInitialized) return false;
        if (!isActive) return false;

        BroadcastMessage("OnReturnedToPool", SendMessageOptions.DontRequireReceiver);

        ResetDynamicState();
        gameObject.SetActive(false);

        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;

        isActive = false;
        currentAssignmentId = null;
        return true;
    }

    private void ResetDynamicState()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;      
            rb.angularVelocity = Vector3.zero;
        }

        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;     
            rb2d.angularVelocity = 0f;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }

    public void Release(float delay = 0f)
    {
        if (owner == null) { Debug.LogWarning($"{nameof(EnemyPoolMember)} on {name} has no owner pool."); return; }
        if (!isActive) return;
        owner.Return(this, delay);
    }
}
