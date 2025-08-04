using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHp = 20;
    [SerializeField] private bool useKnockback = true;

    [Header("Death")]
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float removeDelay = 2.0f; 

    private int hp;
    private int myId;
    private bool isDead;

    private Rigidbody rb;
    private CharacterController cc;
    private Animator anim;

    private int deathTriggerHash;

    private void Awake()
    {
        myId = gameObject.GetInstanceID();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();  
        anim = GetComponent<Animator>();

        deathTriggerHash = Animator.StringToHash(deathTriggerName);
    }

    private void OnEnable()
    {
        hp = maxHp;
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void OnDamage(DamageEvent e)
    {
        if (isDead) return;                 
        if (e.targetId != myId) return;     

        BlockController bc = GetComponent<BlockController>();
        bool blocked = bc != null && bc.IsBlocking && IsFacingAttacker(e.attackerId);

        if (blocked)
        {
            Debug.Log($"{name} ► Blocked!");
            SpawnBlockSpark();              
            return;                        
        }

        hp -= e.amount;
        Debug.Log($"{name} ► HP: {hp}");

        if (useKnockback && e.knockback > 0f)
            ApplyKnockback(e.attackerId, e.knockback);

        if (hp <= 0)
            Die(e.attackerId);
    }

    private bool IsFacingAttacker(int attackerId)
    {
        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk))
            return false;

        Vector3 dir = (atk.position - transform.position).normalized; 
        float dp = Vector3.Dot(transform.forward, dir);           
        return dp > 0.3f;  
    }

    [SerializeField] private GameObject blockEffectPrefab;

    private void SpawnBlockSpark()
    {
        if (blockEffectPrefab == null) return;

        Vector3 pos = transform.position + Vector3.up * 1.2f;
        Instantiate(blockEffectPrefab, pos, Quaternion.identity);
    }


    private void ApplyKnockback(int attackerId, float force)
    {
        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk))
            return;

        Vector3 dir = (transform.position - atk.position).normalized;
        dir.y = 0f;

        if (rb != null)
            rb.AddForce(dir * force, ForceMode.Impulse);
        else if (cc != null)
            cc.Move(dir * force * 0.05f);
    }

    private void Die(int killerId)
    {
        if (isDead) return;
        isDead = true;

        if (CompareTag("Player"))
            CombatBus.Publish(new PlayerDownEvent(myId, killerId));
        else
            CombatBus.Publish(new EnemyDownEvent(myId, killerId));

        if (anim != null)
            anim.SetTrigger(deathTriggerHash);

        DisableCollisions();

        StartCoroutine(RemoveAfterDelay());
    }

    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(removeDelay);
        Destroy(gameObject);
    }

    private void DisableCollisions()
    {
        // Rigidbody
        if (rb != null)
            rb.isKinematic = true;

        // CharacterController
        if (cc != null)
            cc.enabled = false;

        // Colliders
        Collider[] cols = GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;
    }
}

public struct EnemyDownEvent
{
    public int enemyId;
    public int killerId;
    public EnemyDownEvent(int e, int k) { enemyId = e; killerId = k; }
}

public struct PlayerDownEvent
{
    public int playerId;
    public int killerId;
    public PlayerDownEvent(int p, int k) { playerId = p; killerId = k; }
}
