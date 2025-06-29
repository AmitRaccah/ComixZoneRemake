using System.Collections.Generic;
using UnityEngine;

public class AttackActivator : MonoBehaviour
{
    [SerializeField] private Transform leftHandSocket;
    [SerializeField] private Transform rightHandSocket;
    [SerializeField] private Transform leftFootSocket;
    [SerializeField] private Transform rightFootSocket;

    [SerializeField] private AttackData[] attacks;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Animator animator;

    private readonly Dictionary<string, AttackData> map = new();
    public static readonly Dictionary<int, Transform> TransformsById = new();

    HitboxController activeHitbox;
    int myId;

    private void Awake()
    {
        myId = GetInstanceID();

        foreach (AttackData a in attacks)
            if (!map.ContainsKey(a.attackName))
                map.Add(a.attackName, a);
    }

    private void OnEnable() => TransformsById[gameObject.GetInstanceID()] = transform;
    private void OnDisable() => TransformsById.Remove(gameObject.GetInstanceID());

    public void BeginHitbox(string attackName)
    {
        if (activeHitbox) return;           
        if (!map.TryGetValue(attackName, out var data)) return;

        var side = animator.GetBool("Mirror") ? GetMirroredSide(data.side) : data.side;
        var socket = GetSocketForSide(side);
        if (!socket) return;

        var go = Instantiate(hitboxPrefab);
        activeHitbox = go.GetComponent<HitboxController>();
        activeHitbox.Init(data, socket);
        activeHitbox.OnFirstHit += KillHitbox;    

        CombatBus.Publish(new AttackStartedEvent(myId));
    }

    public void EndHitbox() => KillHitbox();

    void KillHitbox()
    {
        if (!activeHitbox) return;
        activeHitbox.OnFirstHit -= KillHitbox;
        Destroy(activeHitbox.gameObject);
        activeHitbox = null;

        CombatBus.Publish(new AttackEndedEvent(myId));
    }

    private Transform GetSocketForSide(AttackSide side)
    {
        switch (side)
        {
            case AttackSide.LeftHand: return leftHandSocket;
            case AttackSide.RightHand: return rightHandSocket;
            case AttackSide.LeftFoot: return leftFootSocket;
            case AttackSide.RightFoot: return rightFootSocket;
            default: return null;
        }
    }

    private AttackSide GetMirroredSide(AttackSide side)
    {
        switch (side)
        {
            case AttackSide.LeftHand: return AttackSide.RightHand;
            case AttackSide.RightHand: return AttackSide.LeftHand;
            case AttackSide.LeftFoot: return AttackSide.RightFoot;
            case AttackSide.RightFoot: return AttackSide.LeftFoot;
            default: return side;
        }
    }
}
