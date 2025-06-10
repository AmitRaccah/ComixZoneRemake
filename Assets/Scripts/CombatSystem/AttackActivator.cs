using System.Collections.Generic;
using UnityEngine;

public class AttackActivator : MonoBehaviour
{
    [SerializeField] private Transform leftHandSocket;
    [SerializeField] private Transform rightHandSocket; 

    [SerializeField] private AttackData[] attacks;
    [SerializeField] private GameObject hitboxPrefab;
    [SerializeField] private Animator animator;      

    Dictionary<string, AttackData> map = new();

    public static readonly Dictionary<int, Transform> TransformsById = new();

    void Awake()
    {
        foreach (var a in attacks)
            if (!map.ContainsKey(a.attackName))
                map.Add(a.attackName, a);
    }

    void OnEnable() => TransformsById[gameObject.GetInstanceID()] = transform;
    void OnDisable() => TransformsById.Remove(gameObject.GetInstanceID());

    /* ---------- ComboParser ---------- */
    public void ActivateAttack(string name)
    {
        if (!map.TryGetValue(name, out var data))
        {
            return;
        }

        Transform baseSocket = data.side == AttackSide.Left ? leftHandSocket
                                                    : rightHandSocket;


        bool mirror = animator.GetBool("Mirror");        //looking left
        Transform socket = mirror ? (baseSocket == leftHandSocket ? rightHandSocket : leftHandSocket)
                              : baseSocket;

        GameObject go = Instantiate(hitboxPrefab);
        go.GetComponent<HitboxController>().Init(data, socket);

        CombatBus.Publish(new AttackPerformedEvent(name, gameObject.GetInstanceID(),data.activeTime));
    }
}
