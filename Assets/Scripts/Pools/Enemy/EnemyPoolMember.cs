using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemySpawnSequence))]
public class EnemyPoolMember : MonoBehaviour
{
    private bool isActive;
    private string currentAssignmentId;
    public string currentEncounterId { get; private set; }
    private Vector3 initialLocalScale;
    private Quaternion initialLocalRotation;

    internal bool IsActive => isActive;
    internal string LastAssignmentId => currentAssignmentId;

    private void Awake()
    {
        initialLocalScale = transform.localScale;
        initialLocalRotation = transform.localRotation;
        gameObject.SetActive(false);
    }

    public void PrepareForSpawn(Transform spawnPoint, string assignmentId, string encounterId)
    {
        if (isActive) return;

        this.currentEncounterId = encounterId;

        transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
        transform.localScale = initialLocalScale;

        ResetDynamicState();
        gameObject.SetActive(true);

        isActive = true;
        currentAssignmentId = assignmentId;

        BroadcastMessage("OnSpawnedFromPool", SendMessageOptions.DontRequireReceiver);
    }

    internal void ReturnToPool()
    {
        if (!isActive) return;
        BroadcastMessage("OnReturnedToPool", SendMessageOptions.DontRequireReceiver);
        gameObject.SetActive(false);
        isActive = false;
        currentAssignmentId = null;
        currentEncounterId = null;
    }

    private void ResetDynamicState()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        Rigidbody2D rb2d = GetComponent<Rigidbody2D>();
        if (rb2d != null) { rb2d.linearVelocity = Vector2.zero; rb2d.angularVelocity = 0f; }

        Animator animator = GetComponent<Animator>();
        if (animator != null) { animator.Rebind(); animator.Update(0f); }
    }

    public void Release()
    {
        if (!isActive || EnemyPool.Instance == null) return;
        EnemyPool.Instance.ReturnToPool(this);
    }
}