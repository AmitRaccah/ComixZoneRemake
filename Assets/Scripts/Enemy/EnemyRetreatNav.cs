using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BehaviorGraphAgent))]
public class EnemyRetreatNav : MonoBehaviour
{
    [SerializeField] string stepsIntParam = "AI_RetreatSteps";
    [SerializeField] string targetVar = "RetreatTarget";
    [SerializeField] string retreatFlagVar = "IsRetreating";
    [SerializeField] string speedFloatParam = "Speed";
    [SerializeField] float stepDistance = 0.8f;
    [SerializeField] float arrivalThreshold = 0.2f;
    [SerializeField] float speedDampTime = 0.1f;
    [SerializeField] LayerMask obstacleMask = ~0;
    [SerializeField] float skin = 0.05f;

    Animator anim;
    BehaviorGraphAgent agent;
    Transform retreatTarget;
    Transform player;
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
        EnsureTarget();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastPos = transform.position;
    }

    void OnEnable() => Reinit();

    void OnDisable()
    {
        if (retreatTarget)
        {
            Destroy(retreatTarget.gameObject);
            retreatTarget = null;
        }
    }

    void OnSpawnedFromPool() => Reinit();

    void Reinit()
    {
        EnsureTarget();
        isRetreating = false;
        agent.SetVariableValue(retreatFlagVar, false);
        anim.SetInteger(stepsId, 0);
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player) agent.SetVariableValue(targetVar, player.gameObject);
    }

    void EnsureTarget()
    {
        if (!retreatTarget)
        {
            var go = new GameObject("RetreatTarget");
            go.hideFlags = HideFlags.HideInHierarchy;
            retreatTarget = go.transform;
        }
    }

    float AllowedBack(Vector3 dir, float want)
    {
        Vector3 nDir = dir.sqrMagnitude > 0f ? dir.normalized : Vector3.zero;

        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            float r = cc.radius;
            float h = Mathf.Max(cc.height - 2f * r, 0.01f);
            Vector3 p1 = transform.position + Vector3.up * r;
            Vector3 p2 = transform.position + Vector3.up * (r + h);
            if (Physics.CapsuleCast(p1, p2, r, nDir, out var hit, want, obstacleMask, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0f, hit.distance - skin);
            return want;
        }

        var cap = GetComponent<CapsuleCollider>();
        if (cap)
        {
            float r = cap.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.z);
            Vector3 c = transform.TransformPoint(cap.center);
            float half = Mathf.Max(cap.height * 0.5f - cap.radius, 0f);
            Vector3 up = transform.up * half;
            Vector3 p1 = c + up;
            Vector3 p2 = c - up;
            if (Physics.CapsuleCast(p1, p2, r, nDir, out var hit2, want, obstacleMask, QueryTriggerInteraction.Ignore))
                return Mathf.Max(0f, hit2.distance - skin);
            return want;
        }

        Vector3 origin = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(origin, nDir, out var rh, want, obstacleMask, QueryTriggerInteraction.Ignore))
            return Mathf.Max(0f, rh.distance - skin);

        return want;
    }

    void Update()
    {
        int steps = anim.GetInteger(stepsId);
        if (steps > 0)
        {
            anim.SetInteger(stepsId, 0);

            Vector3 backDir = Axis2D.BackDir(transform, player);
            float want = steps * stepDistance;
            float allowed = AllowedBack(backDir, want);

            if (allowed <= 0.001f)
            {
                isRetreating = false;
                agent.SetVariableValue(retreatFlagVar, false);
                return;
            }

            Vector3 pos = transform.position + backDir * allowed;
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
        Vector3 delta = transform.position - lastPos; delta.y = 0f;
        float signedSpeed = Time.deltaTime > 0f ? Vector3.Dot(transform.forward, delta) / Time.deltaTime : 0f;
        anim.SetFloat(speedId, signedSpeed, speedDampTime, Time.deltaTime);
        lastPos = transform.position;
    }
}
