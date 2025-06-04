using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float pickupRange = 2f;

 //   [SerializeField] private LayerMask playerLayerMask;

    private Transform player;             

    private void OnEnable() =>
        EventBus.Subscribe<PlayerPickUpEvent>(TryPickup);

    private void OnDisable() =>
        EventBus.Unsubscribe<PlayerPickUpEvent>(TryPickup);

    private void TryPickup(PlayerPickUpEvent _)
    {
        // ② השתמש במסיכה
        Collider[] hits = Physics.OverlapSphere(transform.position,
                                                pickupRange //,
                                                //playerLayerMask
                                                );

        if (hits.Length == 0) return;

        EventBus.Publish(new ItemPickedUpEvent(pickupType));
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
