using UnityEngine;

[CreateAssetMenu(fileName = "New Knife", menuName = "Inventory/Knife")]
public class KnifeItem : Item
{
    [SerializeField] private AttackData attackData;
    public string poolId;
    public float speed = 10f;
    public float distance = 15f;
    public Vector3 spinPerSecond = new Vector3(720f, 0f, 0f);

    public AttackData Data => attackData;

    public override bool Use()
    {
        if (attackData == null || string.IsNullOrEmpty(poolId)) return false;
        CoreBus.Publish(new KnifeThrownEvent(poolId, attackData, speed, distance, spinPerSecond));
        return true;
    }
}
