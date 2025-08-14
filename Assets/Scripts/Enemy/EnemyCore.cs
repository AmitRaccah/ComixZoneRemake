using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyCore : MonoBehaviour
{
    public Animator Anim { get; private set; }
    public Rigidbody Body { get; private set; }
    public BehaviorGraphAgent AI { get; private set; }

    [Header("Lane-Lock")]
    [SerializeField] private float laneZ = 0f;         

    void Awake()
    {
        Anim = GetComponent<Animator>();
        Body = GetComponent<Rigidbody>();
        AI = GetComponent<BehaviorGraphAgent>();
    }

    void Start()
    {
        Vector3 p = transform.position;
        p.z = laneZ;
        transform.position = p;

        Body.constraints |= RigidbodyConstraints.FreezePositionZ
                          | RigidbodyConstraints.FreezeRotation;   
    }
}
