using UnityEngine;
using DG.Tweening;  // ì-DOTween

[CreateAssetMenu(fileName = "New Knife", menuName = "Inventory/Knife")]
public class KnifeItem : Item
{
    public int damage = 5;          
    public float speed = 10f;      
    public float distance = 15f;  
    public float rotationSpeed = 720f; 

    public override void Use()
    {
        CoreBus.Publish(new KnifeThrownEvent(damage, speed, distance, rotationSpeed));
    }
}