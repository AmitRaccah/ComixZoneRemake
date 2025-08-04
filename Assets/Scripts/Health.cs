using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class Health : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int maxHp = 20;
    [SerializeField] private float defaultHitStun = 0.25f;

    [Header("Death")]
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float removeDelay = 2f;

    private int hp;
    private float stunTimer;
    public bool IsStunned => stunTimer > 0f;

    private int myId;
    private bool isDead;
    private Rigidbody rb;
    private CharacterController cc;
    private Animator anim;
    private int deathHash;

    /* ────────── lifecycle ────────── */

    void Awake()
    {
        myId = gameObject.GetInstanceID();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        deathHash = Animator.StringToHash(deathTriggerName);
    }

    void OnEnable()
    {
        hp = maxHp;
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    void Update()
    {
        if (stunTimer > 0f)
            stunTimer -= Time.deltaTime;
    }

    /* ────────── damage ────────── */

    void OnDamage(DamageEvent e)
    {
        if (isDead || e.targetId != myId) return;

        BlockController bc = GetComponent<BlockController>();
        bool blocked = bc && bc.IsBlocking && IsFacingAttacker(e.attackerId);
        if (blocked) return;

        hp -= e.amount;
        stunTimer = defaultHitStun;

        if (e.knockback > 0f)
            ApplyKnockback(e.attackerId, e.knockback);

        if (hp <= 0)
            Die(e.attackerId);
    }

    bool IsFacingAttacker(int attackerId)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return false;

        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }

    void ApplyKnockback(int attackerId, float force)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
            return;

        Vector3 dir = (transform.position - atk.position).normalized;
        dir.y = 0f;

        if (rb) rb.AddForce(dir * force, ForceMode.Impulse);
        else if (cc) cc.Move(dir * force * 0.05f);
    }

    /* ────────── death ────────── */

    void Die(int killerId)
    {
        if (isDead) return;
        isDead = true;

        if (CompareTag("Player"))
            CombatBus.Publish(new PlayerDownEvent(myId, killerId));
        else
            CombatBus.Publish(new EnemyDownEvent(myId, killerId));

        anim.SetTrigger(deathHash);
        DisableCollisions();

        var tpc = GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc) tpc.enabled = false;

        StartCoroutine(RemoveAfterDelay());
    }

    IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(removeDelay);
        Destroy(gameObject);
    }

    void DisableCollisions()
    {
        if (rb) rb.isKinematic = true;
        if (cc) cc.enabled = false;

        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;
    }
}

/* events */
public struct EnemyDownEvent { public int enemyId, killerId; public EnemyDownEvent(int e, int k) { enemyId = e; killerId = k; } }
public struct PlayerDownEvent { public int playerId, killerId; public PlayerDownEvent(int p, int k) { playerId = p; killerId = k; } }
