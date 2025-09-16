using UnityEngine;

public enum Faction { Player, Enemy, Neutral }

public class FactionId : MonoBehaviour
{
    public Faction faction = Faction.Enemy;
    public int EntityId { get; private set; }
    void Awake() => EntityId = gameObject.GetInstanceID();
}
