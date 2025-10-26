using UnityEngine;

public class EnemyVfxFollower : MonoBehaviour
{
    [SerializeField] private string vfxId;
    [SerializeField] private Vector3 offset;
    [SerializeField] private bool faceSameRotation = false;

    private VfxPoolMember activeVfx;

    void OnEnable()
    {
        TrySpawnVfx();
    }

    void OnSpawnedFromPool()
    {
        TrySpawnVfx();
    }

    void Update()
    {
        if (activeVfx == null || !activeVfx.IsActive)
        {
            TrySpawnVfx();
            return;
        }

        Transform fxT = activeVfx.transform;
        fxT.position = transform.position + offset;
        if (faceSameRotation)
            fxT.rotation = transform.rotation;
    }

    void OnDisable()
    {
        DespawnVfx();
    }

    void TrySpawnVfx()
    {
        if (activeVfx != null) return;
        if (VfxPoolManager.Instance == null) return;
        if (string.IsNullOrEmpty(vfxId)) return;

        GameObject go = VfxPoolManager.Instance.Spawn(
            vfxId,
            transform.position + offset,
            faceSameRotation ? transform.rotation : Quaternion.identity
        );

        if (go != null)
            activeVfx = go.GetComponent<VfxPoolMember>();
    }

    void DespawnVfx()
    {
        if (activeVfx != null)
        {
            activeVfx.ReturnToPool();
            activeVfx = null;
        }
    }
}
