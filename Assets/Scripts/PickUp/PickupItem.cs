using UnityEngine;
using UnityEngine.InputSystem;

public class PickupItem : MonoBehaviour
{
    [SerializeField] private PickupType pickupType;
    [SerializeField] private float pickupRange = 2;
    [SerializeField] private Key pickupKey = Key.E;

    private Transform player;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[pickupKey].wasPressedThisFrame)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange);

            foreach (var hit in hits) 
            {
                if (hit.CompareTag("Player"))
                {
                    EventBus.Publish(new ItemPickedUpEvent(pickupType));
                    Destroy(gameObject); // CHANGE WITH POOL MANAGER!!!
                    break;
                }
            }
        } 
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
