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

    private AttackData _currentAttack;


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
    public void SetCurrentAttack(AttackData data)
    {
        _currentAttack = data;
        Debug.Log($"[Activator] CurrentAttack ← {data.attackName}");
    }

    public void BeginHitbox()
    {
        if (_currentAttack == null || activeHitbox != null) return;

        var data = _currentAttack;
        bool mirrored = animator.GetBool("Mirror");
        var side = mirrored ? GetMirroredSide(data.side) : data.side;
        var socket = GetSocketForSide(side);
        if (!socket) return;

        var go = Instantiate(hitboxPrefab);
        activeHitbox = go.GetComponent<HitboxController>();
        activeHitbox.Init(data, socket, GetInstanceID());
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
