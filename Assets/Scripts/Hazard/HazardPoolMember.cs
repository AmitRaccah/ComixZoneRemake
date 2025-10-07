using UnityEngine;

[DisallowMultipleComponent]
public class HazardPoolMember : MonoBehaviour
{
    [Tooltip("Pool ID (must match the ID used by the spawner)")]
    public string hazardId;

    public bool IsActive { get; private set; }

    private Vector3 initialLocalScale;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        initialLocalScale = transform.localScale;
        initialLocalRotation = transform.localRotation;
        gameObject.SetActive(false);
    }

    public void PrepareForSpawn(Vector3 position, Quaternion rotation)
    {
        if (IsActive) return;

        gameObject.SetActive(true);
        IsActive = true;

        Debug.Log($"[HazardPoolMember] SetActive(true). activeSelf={gameObject.activeSelf}, activeInHierarchy={gameObject.activeInHierarchy}", this);

        transform.SetPositionAndRotation(position, rotation);
        transform.localScale = initialLocalScale;
        ResetDynamicState();

        BroadcastMessage("OnSpawnedFromHazardPool", SendMessageOptions.DontRequireReceiver);
    }

    public void ReturnToPool()
    {
        if (!IsActive) return;

        BroadcastMessage("OnReturnedToHazardPool", SendMessageOptions.DontRequireReceiver);
        gameObject.SetActive(false);
        IsActive = false;
    }

    private void ResetDynamicState()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

        var rb2d = GetComponent<Rigidbody2D>();
        if (rb2d) { rb2d.linearVelocity = Vector2.zero; rb2d.angularVelocity = 0f; }

        var animator = GetComponent<Animator>();
        if (animator) { animator.Rebind(); animator.Update(0f); }
    }
}
