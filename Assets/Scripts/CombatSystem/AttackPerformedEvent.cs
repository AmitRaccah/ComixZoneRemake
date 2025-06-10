public struct AttackPerformedEvent
{
    public string attackName;
    public int attackerId;
    public float attackDuration;

    public AttackPerformedEvent(string n, int id, float dur)
    {
        attackName = n;
        attackerId = id;
        attackDuration = dur;
    }
}
