using System.Collections.Generic;
using UnityEngine;

public enum Faction { Player, Enemy, Neutral }

public class FactionId : MonoBehaviour
{
    public Faction faction = Faction.Enemy;
    public int EntityId { get; private set; }

    public static readonly Dictionary<int, Transform> Transforms = new();

    void Awake()
    {
        EntityId = gameObject.GetInstanceID();
    }

    void OnEnable()
    {
        Transforms[EntityId] = transform;
    }

    void OnDisable()
    {
        if (Transforms.TryGetValue(EntityId, out var t) && t == transform)
            Transforms.Remove(EntityId);
    }
}
