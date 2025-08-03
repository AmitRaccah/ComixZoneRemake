using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHp = 20;
    [SerializeField] bool useKnockback = true;   

    int hp;
    int myId;
    Rigidbody rb;         
    CharacterController cc;     

    void Awake()
    {
        myId = gameObject.GetInstanceID();
        rb = GetComponent<Rigidbody>();        
        cc = GetComponent<CharacterController>();
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

        if (useKnockback && e.knockback > 0)
            ApplyKnockback(e.attackerId, e.knockback);

        if (hp <= 0) Die(e.attackerId);
    }

    void ApplyKnockback(int attackerId, float force)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return;

        Vector3 dir = (transform.position - atk.position).normalized;
        dir.y = 0;

        if (rb != null)                               
        {
            rb.AddForce(dir * force, ForceMode.Impulse);
        }
        else if (cc != null)                        
        {
            cc.Move(dir * force * 0.05f);
        }
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

public struct EnemyDownEvent { public int enemyId, killerId; public EnemyDownEvent(int e, int k) { enemyId = e; killerId = k; } }
public struct PlayerDownEvent { public int playerId, killerId; public PlayerDownEvent(int p, int k) { playerId = p; killerId = k; } }
