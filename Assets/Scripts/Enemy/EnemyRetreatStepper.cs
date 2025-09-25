using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyRetreatNav : MonoBehaviour
{
    [SerializeField] private string stepsIntParam = "AI_RetreatSteps"; // Animator int
    [SerializeField] private string targetVar = "RetreatTarget";   // Blackboard GameObject
    [SerializeField] private string retreatFlagVar = "IsRetreating";    // Blackboard bool
    [SerializeField] private float stepDistance = 0.8f;              // מרחק צעד
    [SerializeField] private float arrivalThreshold = 0.2f;             // תואם ל-Navigate Distance Threshold

    private Animator anim;
    private BehaviorGraphAgent agent;
    private int stepsId;
    private Transform retreatTarget;
    private Transform player;
    private bool isRetreating;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<BehaviorGraphAgent>();
        stepsId = Animator.StringToHash(stepsIntParam);

        var go = new GameObject("RetreatTarget");
        go.hideFlags = HideFlags.HideInHierarchy;
        retreatTarget = go.transform;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
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

            agent.SetVariableValue<GameObject>(targetVar, retreatTarget.gameObject);
            isRetreating = true;
            agent.SetVariableValue<bool>(retreatFlagVar, true);
        }

        if (isRetreating)
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = retreatTarget.position; b.y = 0f;
            if ((a - b).sqrMagnitude <= (arrivalThreshold * arrivalThreshold))
            {
                isRetreating = false;
                agent.SetVariableValue<bool>(retreatFlagVar, false);
            }
        }
    }

    private void LateUpdate()
    {
        if (!isRetreating || player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
    }
}
