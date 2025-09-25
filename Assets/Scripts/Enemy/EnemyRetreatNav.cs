using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyRetreatNav : MonoBehaviour
{
    [Header("Blackboard / Animator names")]
    [SerializeField] private string stepsIntParam = "AI_RetreatSteps";
    [SerializeField] private string targetVar = "RetreatTarget";
    [SerializeField] private string retreatFlagVar = "IsRetreating";
    [SerializeField] private string speedFloatParam = "Speed";

    [Header("Retreat step")]
    [SerializeField] private float stepDistance = 0.8f;
    [SerializeField] private float arrivalThreshold = 0.2f;

    [Header("Tuning")]
    [SerializeField] private float speedDampTime = 0.1f;

    private Animator anim;
    private BehaviorGraphAgent agent;
    private Transform retreatTarget;

    private int stepsId, speedId;
    private bool isRetreating;
    private float arrivalSqr;
    private Vector3 lastPos;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<BehaviorGraphAgent>();
        stepsId = Animator.StringToHash(stepsIntParam);
        speedId = Animator.StringToHash(speedFloatParam);

        arrivalSqr = arrivalThreshold * arrivalThreshold;

        GameObject go = new GameObject("RetreatTarget");
        go.hideFlags = HideFlags.HideInHierarchy;
        retreatTarget = go.transform;

        lastPos = transform.position;
    }

    private void OnDisable()
    {
        if (retreatTarget != null) Destroy(retreatTarget.gameObject);
    }

    private void Update()
    {
        int steps = anim.GetInteger(stepsId);
        if (steps > 0)
        {
            anim.SetInteger(stepsId, 0);

            Vector3 fwd = transform.forward;
            fwd.y = 0f;
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

    private void LateUpdate()
    {
        Vector3 delta = transform.position - lastPos;
        delta.y = 0f;

        float signedSpeed = 0f;
        if (Time.deltaTime > 0f)
            signedSpeed = Vector3.Dot(transform.forward, delta) / Time.deltaTime;

        anim.SetFloat(speedId, signedSpeed, speedDampTime, Time.deltaTime);
        lastPos = transform.position;
    }
}
