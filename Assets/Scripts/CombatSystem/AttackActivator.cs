using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Combat/Attack/Attack Activator")]
public class AttackActivator : MonoBehaviour
{
    [SerializeField] private Transform leftHandSocket;
    [SerializeField] private Transform rightHandSocket;
    [SerializeField] private Transform leftFootSocket;
    [SerializeField] private Transform rightFootSocket;

    [SerializeField] private AttackData[] attacks;
    [SerializeField] private Animator animator;

    private AttackData _currentAttack;
    private readonly Dictionary<string, AttackData> map = new();

    public static readonly Dictionary<int, Transform> TransformsById = new();

    private HitboxController activeHitbox;
    private int myId;

    void Awake()
    {
        myId = GetInstanceID();
        for (int i = 0; i < attacks.Length; i++)
        {
            var a = attacks[i];
            if (a != null && !string.IsNullOrEmpty(a.attackName) && !map.ContainsKey(a.attackName))
                map.Add(a.attackName, a);
        }
    }

    void OnEnable() { TransformsById[myId] = transform; }
    void OnDisable()
    {
        TransformsById.Remove(myId);
        if (activeHitbox != null)
        {
            activeHitbox.OnFirstHit -= KillHitbox;
            activeHitbox.Despawn();
            activeHitbox = null;
        }
    }

    public void SetCurrentAttack(AttackData data)
    {
        _currentAttack = data;
#if UNITY_EDITOR
        if (data) Debug.Log($"[Activator] CurrentAttack ← {data.attackName}", this);
#endif
    }

    public void BeginHitbox(string attackKey = "")
    {
        AttackData data = null;
        if (!string.IsNullOrEmpty(attackKey))
            map.TryGetValue(attackKey, out data);
        if (data == null) data = _currentAttack;
        if (data == null) return;

        bool mirrored = animator && animator.GetBool("Mirror");
        var side = mirrored ? GetMirroredSide(data.side) : data.side;
        var socket = GetSocketForSide(side);
        if (!socket || HitboxPool.Instance == null) return;

        CombatBus.Publish(new AttackStartedEvent(myId, data, socket));

        activeHitbox = HitboxPool.Instance.Spawn(socket, data, myId);
        if (!activeHitbox) return;

        activeHitbox.OnFirstHit += KillHitbox;
    }

    public void EndHitbox() => KillHitbox();

    private void KillHitbox()
    {
        if (!activeHitbox) return;
        activeHitbox.OnFirstHit -= KillHitbox;
        activeHitbox.Despawn();
        activeHitbox = null;
        _currentAttack = null;
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
