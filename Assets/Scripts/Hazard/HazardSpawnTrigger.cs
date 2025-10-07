using UnityEngine;

[AddComponentMenu("Hazard/Sweep Spawn Trigger (Bus)")]
public class HazardSpawnTrigger : MonoBehaviour
{
    [SerializeField] private string hazardId = "1";
    [SerializeField] private bool fromLeft = true;
    [SerializeField] private float warningLeadTime = 0f;
    [SerializeField] private bool oneShot = true;

    private bool fired;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (oneShot && fired) return;

        var side = fromLeft ? HazardSide.Left : HazardSide.Right;
        Debug.Log($"[HazardTrigger] PUBLISH request id={hazardId}, side={side}, warn={warningLeadTime}", this);

        CoreBus.Publish(new HazardSweepRequest(hazardId, side, warningLeadTime));
        fired = true;
    }
}
