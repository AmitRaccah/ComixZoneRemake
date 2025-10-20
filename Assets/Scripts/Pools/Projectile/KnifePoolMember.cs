using UnityEngine;

[RequireComponent(typeof(KnifeProjectile))]
public class KnifePoolMember : MonoBehaviour
{
    public string knifeId;
    public bool IsActive { get; private set; }

    private KnifeProjectile projectile;

    void Awake()
    {
        projectile = GetComponent<KnifeProjectile>();
        gameObject.SetActive(false);
        IsActive = false;
    }

    public void PrepareForSpawn(
        Vector3 position,
        Quaternion rotation,
        int attackerId,
        AttackData data,
        float speed,
        float distance,
        Vector3 spinPerSecond)
    {
        if (IsActive) return;

        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);
        IsActive = true;

        projectile.Activate(this, attackerId, data, speed, distance, spinPerSecond);
    }

    public void ReturnToPool()
    {
        projectile.Deactivate();
        IsActive = false;
        gameObject.SetActive(false);
    }
}
