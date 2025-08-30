using UnityEngine;
using UnityEngine.InputSystem;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float pickupRange = 2f;


    //   [SerializeField] private LayerMask playerLayerMask;


    private Transform player;

    private void Awake()
    {
    }

    private void OnEnable() =>
        CoreBus.Subscribe<PlayerPickUpEvent>(TryPickup);

    private void OnDisable() =>
        CoreBus.Unsubscribe<PlayerPickUpEvent>(TryPickup);

    private void TryPickup(PlayerPickUpEvent _)
    {
        if (!IsPlayerInRange()) return;

        bool accepted = InventoryManager.Instance.TryAddItem(pickupType);

        if (accepted)
        {
            AnimationHelper.Instance?.Trigger("Pickup");
            Debug.Log("Picked up! " + pickupType);
            Destroy(gameObject);
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

