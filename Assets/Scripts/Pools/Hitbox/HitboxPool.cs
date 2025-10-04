using System.Collections.Generic;
using UnityEngine;

public class HitboxPool : MonoBehaviour
{
    public static HitboxPool Instance { get; private set; }

    [Tooltip("Optional: drag pre-placed HitboxController instances here. If left empty, children will be auto-discovered.")]
    [SerializeField] private List<HitboxController> preplaced = new();

    private readonly Stack<HitboxController> free = new();

    void Awake()
    {
        Instance = this;

        if (preplaced == null || preplaced.Count == 0)
        {
            preplaced = new List<HitboxController>(GetComponentsInChildren<HitboxController>(true));
        }

        for (int i = 0; i < preplaced.Count; i++)
        {
            var hb = preplaced[i];
            if (!hb) continue;

            hb.AssignPool(this);
            hb.gameObject.SetActive(false);
            hb.transform.SetParent(transform, false);
            free.Push(hb);
        }
    }

    public HitboxController Spawn(Transform socket, AttackData data, int attackerId)
    {
        if (free.Count == 0)
        {
            Debug.LogWarning("HitboxPool: capacity exhausted. Add more pre-placed hitboxes to the scene.", this);
            return null;
        }

        var hb = free.Pop();
        hb.gameObject.SetActive(true);
        hb.Init(data, socket, attackerId); 
        return hb;
    }

    public void Release(HitboxController hb)
    {
        if (!hb) return;
        hb.gameObject.SetActive(false);
        hb.transform.SetParent(transform, false);
        free.Push(hb);
    }
}
