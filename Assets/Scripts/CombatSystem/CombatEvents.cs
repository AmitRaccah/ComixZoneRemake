public struct DamageEvent
{
    public int attackerId;
    public int targetId;
    public int amount;
    public float knockback;
    public DamageType type;

    public float shakeAmplitude;
    public float freezeFrameDuration;

    //VFX
    public AttackData attackData;

}

public struct AttackPerformedEvent
{
    public string attackName;
    public int attackerId;

    public AttackPerformedEvent(string n, int id)
    {
        attackName = n;
        attackerId = id;
    }
}

public struct EnemyDownEvent
{
    public int enemyId, killerId;
    public EnemyDownEvent(int e, int k)
    {
        enemyId = e;
        killerId = k;
    }
}

public struct PlayerDownEvent
{
    public int playerId, killerId;
    public PlayerDownEvent(int p, int k)
    {
        playerId = p;
        killerId = k;
    }
}

public struct KnockbackEvent
{
    public int targetId;
    public int attackerId;
    public float force;
}

[System.Serializable]
public struct Step
{
    public InputType input;
    public PlayerStance stance;
    public string trigger;
    public AttackData attack;
}

public struct HealthChangedEvent
{
    public int entityId;
    public int current;
    public int max;
    public bool isDead;

    public HealthChangedEvent(int entityId, int current, int max, bool isDead = false)
    {
        this.entityId = entityId;
        this.current = current;
        this.max = max;
        this.isDead = isDead;
    }
}


public struct HealthDepletedEvent
{
    public int entityId;
    public Faction faction;
    public int killerId;
    public HealthDepletedEvent(int id, Faction f, int killer) { entityId = id; faction = f; killerId = killer; }
}