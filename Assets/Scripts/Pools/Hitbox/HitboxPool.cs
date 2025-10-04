using System.Collections.Generic;
using UnityEngine;

public class HitboxPool : MonoBehaviour
{
    public static HitboxPool Instance { get; private set; }

    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private int prewarm = 16;

    private readonly Stack<HitboxController> free = new();

    void Awake()
    {
        Instance = this;
        for (int i = 0; i < Mathf.Max(0, prewarm); i++)
            free.Push(CreateOne());
    }

    HitboxController CreateOne()
    {
        var go = Instantiate(hitboxPrefab, transform);
        go.SetActive(false);
        var hb = go.GetComponent<HitboxController>();
        hb.AssignPool(this);
        return hb;
    }

    public HitboxController Spawn(Transform socket, AttackData data, int attackerId)
    {
        var hb = (free.Count > 0) ? free.Pop() : CreateOne();
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
