using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "New Knife", menuName = "Inventory/Knife")]
public class KnifeItem : Item
{
    [SerializeField] private AttackData attackData;
    public float speed = 10f;
    public float distance = 15f;
    public float rotationSpeed = 720f;
    public override bool Use()
    {
        if (attackData == null) return false;
        CoreBus.Publish(new KnifeThrownEvent(attackData, speed, distance, rotationSpeed));
        return true;
    }
}