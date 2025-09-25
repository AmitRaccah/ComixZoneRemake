
using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyRetreatNav : MonoBehaviour
{
    [Header("Blackboard / Animator names")]
    [SerializeField] string stepsIntParam = "AI_RetreatSteps"; 
    [SerializeField] string targetVar = "RetreatTarget";   
    [SerializeField] string retreatFlagVar = "IsRetreating";    
    [SerializeField] string speedFloatParam = "Speed";          

    [Header("Retreat step")]
    [SerializeField] float stepDistance = 0.8f; 
    [SerializeField] float arrivalThreshold = 0.2f;

    [Header("Tuning")]
    [SerializeField] float speedDampTime = 0.1f;

    Animator anim;
    BehaviorGraphAgent agent;
    Transform retreatTarget, player;

    int stepsId, speedId;
    bool isRetreating;
    float arrivalSqr;
    Vector3 lastPos;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<BehaviorGraphAgent>();
        stepsId = Animator.StringToHash(stepsIntParam);
        speedId = Animator.StringToHash(speedFloatParam);

        arrivalSqr = arrivalThreshold * arrivalThreshold;

        var go = new GameObject("RetreatTarget");
        go.hideFlags = HideFlags.HideInHierarchy;
        retreatTarget = go.transform;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) player = p.transform;

        lastPos = transform.position;
    }

    void OnDisable()
    {
        if (retreatTarget) Destroy(retreatTarget.gameObject);
    }

    void Update()
    {
        int steps = anim.GetInteger(stepsId);
        if (steps > 0)
        {
            anim.SetInteger(stepsId, 0);

            Vector3 fwd = transform.forward; fwd.y = 0f;
            if (fwd.sqrMagnitude > 0f) fwd.Normalize(); else fwd = Vector3.right;

            Vector3 pos = transform.position - fwd * (steps * stepDistance);
            pos.z = transform.position.z;
            retreatTarget.position = pos;

            agent.SetVariableValue(targetVar, retreatTarget.gameObject);

            if (!isRetreating)
            {
                isRetreating = true;
                agent.SetVariableValue(retreatFlagVar, true);
            }
        }

        if (isRetreating)
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = retreatTarget.position; b.y = 0f;
            if ((a - b).sqrMagnitude <= arrivalSqr)
            {
                isRetreating = false;
                agent.SetVariableValue(retreatFlagVar, false);
            }
        }
    }

    void LateUpdate()
    {
        if (isRetreating && player)
        {
            Vector3 toPlayer = player.position - transform.position; toPlayer.y = 0f;
            if (toPlayer.sqrMagnitude > 1e-4f)
                transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        }

        Vector3 delta = transform.position - lastPos; delta.y = 0f;
        float signedSpeed = (Time.deltaTime > 0f)
            ? Vector3.Dot(transform.forward, delta) / Time.deltaTime
            : 0f;

        anim.SetFloat(speedId, signedSpeed, speedDampTime, Time.deltaTime);
        lastPos = transform.position;
    }
}


