﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 20;

    int hp;
    Rigidbody rb;

    void Awake() => rb = GetComponent<Rigidbody>();

    void OnEnable()
    {
        hp = maxHp;
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    void OnDamage(DamageEvent e)
    {
        if (e.targetId != gameObject.GetInstanceID()) return;

        hp -= e.amount;
        Debug.Log($"{name} ► HP: {hp}");

        if (e.knockback > 0f)
            ApplyKnockback(e.attackerId, e.knockback);

        if (hp <= 0) Die(e.attackerId);
    }

    void ApplyKnockback(int attackerId, float force)
    {
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var attacker))
        {
            Debug.LogWarning("Knockback: attacker not found");
            return;
        }

        Vector3 dir = (transform.position - attacker.position).normalized;
        dir.y = 0f;
        rb.AddForce(dir * force, ForceMode.Impulse);
    }

    void Die(int killerId)
    {
        CombatBus.Publish(new EnemyDownEvent(gameObject.GetInstanceID(), killerId));
        Destroy(gameObject);
    }
}

public struct EnemyDownEvent
{
    public int enemyId, killerId;
    public EnemyDownEvent(int e, int k) { enemyId = e; killerId = k; }
}
