using UnityEngine;
using DG.Tweening;

[CreateAssetMenu(fileName = "New Knife", menuName = "Inventory/Knife")]
public class KnifeItem : Item
{
    [SerializeField] private AttackData attackData;
    public float speed = 10f;
    public float distance = 15f;
    public float rotationSpeed = 720f;
    public override void Use()
    {
        if (attackData == null) return;
        CoreBus.Publish(new KnifeThrownEvent(attackData, speed, distance, rotationSpeed)); 
    }
}