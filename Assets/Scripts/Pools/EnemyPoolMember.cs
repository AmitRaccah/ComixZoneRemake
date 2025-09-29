using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool owner;
    private GameObject sourcePrefab;
    private bool isInitialized;
    private bool isActive;
    private Vector3 initialLocalScale;
    private Quaternion initialLocalRotation;
    private string currentAssignmentId;
    private bool isQueued;

    internal GameObject SourcePrefab => sourcePrefab;
    internal bool IsActive => isActive;
    internal bool IsPooled => owner != null;
    internal string LastAssignmentId => currentAssignmentId;
    internal bool IsQueued => isQueued;

    internal void Initialize(EnemyPool pool, GameObject prefab)
    {
        owner = pool;
        sourcePrefab = prefab;
        isInitialized = true;
        initialLocalScale = transform.localScale;
        initialLocalRotation = transform.localRotation;
        isActive = false;
        currentAssignmentId = null;
        isQueued = true;
    }

    internal void PrepareForSpawn(Transform parentOverride, Transform spawnPoint, string assignmentId)
    {
        if (!isInitialized)
        {
            Debug.LogWarning($"EnemyPoolMember on {name} was not initialized before spawn.");
        }

        Transform targetParent = parentOverride != null
            ? parentOverride
            : spawnPoint != null ? spawnPoint.parent : null;

        if (targetParent != null)
        {
            transform.SetParent(targetParent, true);
        }
        else
        {
            transform.SetParent(null, true);
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
        isQueued = false;
        BroadcastMessage("OnSpawnedFromPool", SendMessageOptions.DontRequireReceiver);
    }

    internal bool ReturnToPool(Transform poolParent)
    {
        if (!isInitialized) return false;
        if (!isActive) return false;

        BroadcastMessage("OnReturnedToPool", SendMessageOptions.DontRequireReceiver);
        ResetDynamicState();
        gameObject.SetActive(false);
        transform.SetParent(poolParent, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;
        isActive = false;
        currentAssignmentId = null;
        isQueued = true;
        return true;
    }

    private void ResetDynamicState()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    public void Release(float delay = 0f)
    {
        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!isActive) return;

        owner.Release(this, delay);
    }
}