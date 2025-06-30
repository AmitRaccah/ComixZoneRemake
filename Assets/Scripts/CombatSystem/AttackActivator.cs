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

    void Awake()
    {
        myId = GetInstanceID();

        foreach (AttackData a in attacks)
            if (!map.ContainsKey(a.attackName))
                map.Add(a.attackName, a);
    }

    void OnEnable()
    {
        TransformsById[myId] = transform;
    }

    void OnDisable()
    {
        TransformsById.Remove(myId);
    }

    public void BeginHitbox(string attackName)
    {
        if (activeHitbox) return;
        if (!map.TryGetValue(attackName, out AttackData data)) return;

        AttackSide side = animator.GetBool("Mirror") ? GetMirroredSide(data.side) : data.side;
        Transform socket = GetSocketForSide(side);
        if (!socket) return;
        GameObject go = Instantiate(hitboxPrefab);
        activeHitbox = go.GetComponent<HitboxController>();


        activeHitbox.Init(data, socket, myId);
        activeHitbox.OnFirstHit += KillHitbox;
    }

    public void EndHitbox()
    {
        KillHitbox();
    }

    void KillHitbox()
    {
        if (!activeHitbox) return;
        activeHitbox.OnFirstHit -= KillHitbox;
        Destroy(activeHitbox.gameObject);
        activeHitbox = null;
    }

    Transform GetSocketForSide(AttackSide side)
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

    AttackSide GetMirroredSide(AttackSide side)
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
