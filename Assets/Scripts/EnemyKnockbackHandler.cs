using UnityEngine;
using System.Collections;
using Unity.Behavior;

public class EnemyKnockbackHandler : MonoBehaviour
{
    private Rigidbody rb;
    private BehaviorGraphAgent agent;
    private int myId;
    [SerializeField] private float knockbackDuration = 0.25f;  
    [SerializeField] private float dampingFactor = 5f; 

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<BehaviorGraphAgent>();
        myId = gameObject.GetInstanceID();
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<KnockbackEvent>(this.OnKnockback);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<KnockbackEvent>(this.OnKnockback);
    }

    private void OnKnockback(KnockbackEvent e)
    {
        if (e.targetId != myId)
        {
            return;
        }
        Transform attackerTransform;
        if (!AttackActivator.TransformsById.TryGetValue(e.attackerId, out attackerTransform))
        {
            return;
        }
        Vector3 dir = (transform.position - attackerTransform.position).normalized;
        dir.y = 0f;
        rb.AddForce(dir * e.force, ForceMode.Impulse);

        if (agent != null)
        {
            agent.SetVariableValue("IsKnockback", true);
        }

        StartCoroutine(this.DampenKnockback());
    }

    private IEnumerator DampenKnockback()
    {
        float timer = knockbackDuration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            Vector3 vel = rb.linearVelocity;
            vel.x = Mathf.Lerp(vel.x, 0f, Time.deltaTime * dampingFactor);
            vel.z = Mathf.Lerp(vel.z, 0f, Time.deltaTime * dampingFactor);
            rb.linearVelocity = vel;
            yield return null;
        }
        Vector3 finalVel = rb.linearVelocity;
        finalVel.x = 0f;
        finalVel.z = 0f;
        rb.linearVelocity = finalVel;

        if (agent != null)
        {
            agent.SetVariableValue("IsKnockback", false);
        }
    }
}