using UnityEngine;
using System.Collections;
using StarterAssets;

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

    public int EntityId => myId;
    public int CurrentHp => hp;
    public int MaxHp => maxHp;


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
        CoreBus.Subscribe<PotionConsumedEvent>(OnPotionConsumed);

        PublishHealthChanged();
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
        CoreBus.Unsubscribe<PotionConsumedEvent>(OnPotionConsumed);
    }

    void Update()
    {
        if (stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (anim.GetBool("CanAttack"))
            {
                anim.SetBool("CanAttack", false);
            }
        }
        else
        {
            if (!anim.GetBool("CanAttack"))
            {
                anim.SetBool("CanAttack", true);
            }
        }
    }

    void OnDamage(DamageEvent e)
    {
        if (isDead || e.targetId != myId) return;
        BlockController bc = GetComponent<BlockController>();
        bool blocked = bc && bc.IsBlocking && IsFacingAttacker(e.attackerId);
        if (blocked) return;
        hp -= e.amount;
        if (hp < 0) hp = 0;
        stunTimer = defaultHitStun;
        anim.SetBool("CanAttack", false);
        PublishHealthChanged();
        if (e.knockback > 0f)
        {
            KnockbackEvent knockEvent = new KnockbackEvent();
            knockEvent.targetId = myId;
            knockEvent.attackerId = e.attackerId;
            knockEvent.force = e.knockback;
            CombatBus.Publish(knockEvent);
        }
        if (hp <= 0)
        {
            Die(e.attackerId);
        }
    }

    bool IsFacingAttacker(int attackerId)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk))
        {
            return false;
        }
        Vector3 dir = (atk.position - transform.position).normalized;
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }

    void Die(int killerId)
    {
        if (isDead) return;
        isDead = true;

        if (hp != 0) hp = 0;
        PublishHealthChanged(true);

        if (CompareTag("Player"))
        {
            CombatBus.Publish(new PlayerDownEvent(myId, killerId));
        }
        else
        {
            CombatBus.Publish(new EnemyDownEvent(myId, killerId));
        }
        anim.SetTrigger(deathHash);
        DisableCollisions();
        var tpc = GetComponent<ThirdPersonController>();
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
        {
            col.enabled = false;
        }
    }

    private void OnPotionConsumed(PotionConsumedEvent e)
    {
        if (isDead || !CompareTag("Player")) return;

        hp = Mathf.Min(hp + e.healAmount, maxHp);
        AnimationHelper.Instance?.Trigger("Drink");
        Debug.Log($"Healed {e.healAmount} HP. Current HP: {hp}/{maxHp}");

        PublishHealthChanged();
    }

    private void PublishHealthChanged(bool dead = false)
    {
        CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, dead));
        Debug.Log($"[HealthChanged] id={myId} hp={hp}/{maxHp} isPlayer={CompareTag("Player")} dead={dead}");

    }

}