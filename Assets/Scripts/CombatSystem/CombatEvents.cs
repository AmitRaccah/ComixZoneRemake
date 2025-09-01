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