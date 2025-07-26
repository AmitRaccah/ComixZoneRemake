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

    private void Awake()
    {
        foreach (AttackData a in attacks)
            if (!map.ContainsKey(a.attackName))
                map.Add(a.attackName, a);
    }

    private void OnEnable() => TransformsById[gameObject.GetInstanceID()] = transform;
    private void OnDisable() => TransformsById.Remove(gameObject.GetInstanceID());

    public void ActivateAttack(string name)
    {
        if (!map.TryGetValue(name, out AttackData data))
            return;

        Transform socket = GetSocketForSide(data.side);
        if (socket == null) return;   

        if (animator.GetBool("Mirror"))
        {
            AttackSide mirrorSide = GetMirroredSide(data.side);
            socket = GetSocketForSide(mirrorSide);
        }

        GameObject go = Instantiate(hitboxPrefab);
        go.GetComponent<HitboxController>().Init(data, socket);

        CombatBus.Publish(new AttackPerformedEvent(name, gameObject.GetInstanceID()));
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
