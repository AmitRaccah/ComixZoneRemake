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
