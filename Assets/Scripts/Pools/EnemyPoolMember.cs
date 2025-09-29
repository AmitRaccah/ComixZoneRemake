using UnityEngine;

[DisallowMultipleComponent]
public class EnemyPoolMember : MonoBehaviour
{
    private EnemyPool owner;
    private int slotIndex;

    public void Bind(EnemyPool pool, int index)
    {
        owner = pool;
        slotIndex = index;
    }

    public void ReleaseToPool()
    {
        if (owner == null)
        {
            gameObject.SetActive(false);
            return;
        }

        owner.HandleMemberRelease(this, slotIndex);
    }

    public void PrepareForSpawn()
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
    }
}