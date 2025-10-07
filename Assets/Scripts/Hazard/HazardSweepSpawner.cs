using System.Collections;
using UnityEngine;

[AddComponentMenu("Hazard/Sweep Spawner (Bus)")]
public class HazardSweepSpawner : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform leftSpawn;
    [SerializeField] private Transform rightSpawn;

    [Header("Motion (single source of truth)")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float travelDistance = 30f;

    private void OnEnable()
    {
        CoreBus.Subscribe<HazardSweepRequest>(OnRequest);
        Debug.Log("[HazardSpawner] SUBSCRIBED to HazardSweepRequest", this);
    }
    private void OnDisable()
    {
        CoreBus.Unsubscribe<HazardSweepRequest>(OnRequest);
        Debug.Log("[HazardSpawner] UNSUBSCRIBED", this);
    }

    private void OnRequest(HazardSweepRequest e)
    {
        Debug.Log($"[HazardSpawner] RECEIVED request id={e.hazardId}, side={e.side}, warn={e.warningLeadTime}", this);
        StartCoroutine(SpawnRoutine(e));
    }

    private IEnumerator SpawnRoutine(HazardSweepRequest e)
    {
        if (e.warningLeadTime > 0f)
        {
            // אפשר גם לפרסם אזהרה אם תרצה, לא חובה
            yield return new WaitForSeconds(e.warningLeadTime);
        }

        if (!HazardPoolManager.Instance)
        {
            Debug.LogError("[HazardSpawner] HazardPoolManager.Instance is NULL", this);
            yield break;
        }

        Transform sp = (e.side == HazardSide.Left) ? leftSpawn : rightSpawn;
        if (!sp)
        {
            Debug.LogError("[HazardSpawner] Missing spawn point for side=" + e.side, this);
            yield break;
        }

        var go = HazardPoolManager.Instance.Spawn(e.hazardId, sp.position, sp.rotation);
        if (!go)
        {
            Debug.LogWarning($"[HazardSpawner] POOL EMPTY for id='{e.hazardId}'", this);
            yield break;
        }

        var mover = go.GetComponent<HazardSweepMover>();
        if (!mover)
        {
            Debug.LogError("[HazardSpawner] Pooled object missing HazardSweepMover", go);
            yield break;
        }

        int dirSign = (e.side == HazardSide.Left) ? +1 : -1;
        Debug.Log($"[HazardSpawner] SPAWN ok → dirSign={dirSign}, speed={speed}, dist={travelDistance}", go);
        mover.Setup(dirSign, speed, travelDistance);
    }
}
