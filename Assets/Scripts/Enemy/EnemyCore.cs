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
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Body = GetComponent<Rigidbody>();
        AI = GetComponent<BehaviorGraphAgent>();

    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
