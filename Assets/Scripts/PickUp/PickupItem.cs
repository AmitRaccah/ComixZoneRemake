using UnityEngine;
using UnityEngine.InputSystem;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float pickupRange = 2f;

 //   [SerializeField] private LayerMask playerLayerMask;

    private Transform player;             

    private void OnEnable() =>
        CoreBus.Subscribe<PlayerPickUpEvent>(TryPickup);

    private void OnDisable() =>
        CoreBus.Unsubscribe<PlayerPickUpEvent>(TryPickup);

    private void TryPickup(PlayerPickUpEvent _)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

        bool playerFound = false;
        foreach (var h in hits)
        {
            if (h.CompareTag("Player"))
            {
                playerFound = true;
                break;
            }
        }
        if (!playerFound) return;

        CoreBus.Publish(new ItemPickedUpEvent(pickupType));
        Destroy(gameObject);
    }


    private bool PlayerInRange()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player == null) return false;

        return Vector3.Distance(transform.position, player.position) <= pickupRange;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
#endif
}
