using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 20;

    int hp;            
    int myId;          
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        myId = gameObject.GetInstanceID();
    }

    void OnEnable()
    {
        hp = maxHp;
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }
    void OnDisable() =>
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return; 

        hp -= e.amount;
        Debug.Log($"{name} ► HP:{hp}");

        if (e.knockback > 0)
            ApplyKnockback(e.attackerId, e.knockback);

        if (hp <= 0)
            Die(e.attackerId);
    }

    void ApplyKnockback(int attackerId, float force)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return;

        Vector3 dir = (transform.position - atk.position).normalized;
        dir.y = 0f;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    void Die(int killerId)
    {
        if (CompareTag("Player"))
            CombatBus.Publish(new PlayerDownEvent(myId, killerId));
        else
            CombatBus.Publish(new EnemyDownEvent(myId, killerId));

        Destroy(gameObject);
    }
}

public struct EnemyDownEvent
{
    public int enemyId, killerId;
    public EnemyDownEvent(int e, int k) { enemyId = e; killerId = k; }
}

public struct PlayerDownEvent
{
    public int playerId, killerId;
    public PlayerDownEvent(int p, int k) { playerId = p; killerId = k; }
}
