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

    public void PrepareForSpawn(KnifeSpawnParams p)
    {
        if (IsActive) return;
        transform.SetPositionAndRotation(p.startPos, p.startRot);
        gameObject.SetActive(true);
        IsActive = true;
        projectile.Activate(this, p);
    }

    public void ReturnToPool()
    {
        projectile.Deactivate();
        IsActive = false;
        gameObject.SetActive(false);
    }
}
