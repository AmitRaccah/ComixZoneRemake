using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Inventory/Potion")]
public class PotionItem : Item
{
    public int healAmount = 10;

    public override void Use()
    {
        CoreBus.Publish(new PotionConsumedEvent(healAmount));
    }
}
