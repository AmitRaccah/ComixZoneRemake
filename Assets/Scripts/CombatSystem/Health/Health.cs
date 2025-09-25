using UnityEngine;

[RequireComponent(typeof(FactionId))]
public class Health : MonoBehaviour
{
    [SerializeField] private int maxHp = 20;
    private int hp;
    private int myId;
    private FactionId faction;

    public int CurrentHp => hp;
    public int MaxHp => maxHp;
    public bool IsDead => hp <= 0;

    void Awake()
    {
        faction = GetComponent<FactionId>();
        myId = gameObject.GetInstanceID();
    }

    void OnEnable()
    {
        hp = maxHp;
        CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, false));
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }
    void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    public void Heal(int amount)
    {
        if (IsDead || amount <= 0) return;
        int old = hp;
        hp = Mathf.Min(maxHp, hp + amount);
        if (hp != old) CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, false));
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId || IsDead) return;

        if (BlockUtil.IsBlocked(this, e.attackerId)) return;

        hp = Mathf.Max(0, hp - e.amount);
        CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, hp == 0));

        if (hp == 0)
            CoreBus.Publish(new HealthDepletedEvent(myId, faction != null ? faction.faction : Faction.Neutral, e.attackerId));
    }

    [SerializeField] private float deathDelay = 0f;
    public float DeathDelay => deathDelay;

    public int EntityId => myId;

    public void RespawnReset()
    {
        hp = maxHp;
        CoreBus.Publish(new HealthChangedEvent(myId, hp, maxHp, false));
    }
}
