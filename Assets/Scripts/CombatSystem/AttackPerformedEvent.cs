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
