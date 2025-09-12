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
    private int myId;
    private bool isDead;
    private Rigidbody rb;
    private CharacterController cc;
    private Animator anim;
    private int deathHash;

    public int EntityId { get { return myId; } }
    public int CurrentHp { get { return hp; } }
    public int MaxHp { get { return maxHp; } }
    public bool IsStunned { get { return stunTimer > 0f; } }
    public bool IsDead { get { return isDead; } }

    private void Awake()
    {
        myId = gameObject.GetInstanceID();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        deathHash = Animator.StringToHash(deathTriggerName);
    }

    private void OnEnable()
    {
        hp = maxHp;
        PublishHealthChanged(false);
    }

    private void Update()
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

    // --- API ציבורי ---
    public void Heal(int amount)
    {
        if (isDead) return;
        if (amount <= 0) return;

        int old = hp;
        hp = Mathf.Min(hp + amount, maxHp);
        if (hp != old)
        {
            PublishHealthChanged(false);
        }
    }

    public void TakeDamage(int amount, int attackerId, float knockback)
    {
        if (isDead) return;
        if (amount <= 0) return;

        hp -= amount;
        if (hp < 0) hp = 0;

        stunTimer = defaultHitStun;
        anim.SetBool("CanAttack", false);

        PublishHealthChanged(false);

        if (knockback > 0f)
        {
            KnockbackEvent k = new KnockbackEvent();
            k.targetId = myId;
            k.attackerId = attackerId;
            k.force = knockback;
            CombatBus.Publish(k);
        }

        if (hp <= 0)
        {
            Die(attackerId);
        }
    }
    // -------------------

    private void Die(int killerId)
    {
        if (isDead) return;
        isDead = true;

        if (hp != 0)
        {
            hp = 0;
        }
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

        ThirdPersonController tpc = GetComponent<ThirdPersonController>();
        if (tpc != null) tpc.enabled = false;

        StartCoroutine(RemoveAfterDelay());
    }

    private IEnumerator RemoveAfterDelay()
    {
         yield return new WaitForSeconds(removeDelay);
        Destroy(gameObject);
    }

    private void DisableCollisions()
    {
        if (rb != null) rb.isKinematic = true;
        if (cc != null) cc.enabled = false;

        Collider[] cols = GetComponentsInChildren<Collider>();
        for (int i = 0; i < cols.Length; i++)
        {
            cols[i].enabled = false;
        }
    }

    private void PublishHealthChanged(bool dead)
    {
        CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, dead));
    }
}
