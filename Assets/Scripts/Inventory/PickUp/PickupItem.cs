using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float pickupRange = 2f;

    private void OnEnable() =>
        CoreBus.Subscribe<PlayerPickUpEvent>(TryPickup);

    private void OnDisable() =>
        CoreBus.Unsubscribe<PlayerPickUpEvent>(TryPickup);

    private void TryPickup(PlayerPickUpEvent _)
    {
        if (!IsPlayerInRange()) return;

        Item def = InventoryManager.Instance.GetItemDefinition(pickupType);

        bool accepted = InventoryManager.Instance.TryAddItem(pickupType);

        var sfx = FindObjectOfType<SfxPlayer>();

        if (accepted)
        {
            if (sfx && def && def.pickupSuccessCue != null)
                sfx.Play(def.pickupSuccessCue, transform.position, null);

            AnimationHelper.Instance?.Trigger("Pickup");
            Destroy(gameObject);
        }
        else
        {
            if (sfx && def && def.pickupFailCue != null)
                sfx.Play(def.pickupFailCue, transform.position, null);
        }
    }

    bool IsPlayerInRange()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);
        for (int i = 0; i < hits.Length; i++)
            if (hits[i].CompareTag("Player"))
                return true;
        return false;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
#endif
}
