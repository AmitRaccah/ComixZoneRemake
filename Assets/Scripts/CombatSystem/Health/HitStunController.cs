using UnityEngine;
using System.Collections;

public class HitStunController : MonoBehaviour
{
    [SerializeField] private float defaultHitStun = 0.25f;
    public bool IsStunned { get; private set; }
    private int myId;

    void Awake() => myId = gameObject.GetInstanceID();
    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    private void OnDamage(DamageEvent e)
    {
        if (e.targetId != myId) return;

        if (IsBlockedAgainst(e.attackerId)) return;

        float dur = (e.freezeFrameDuration > 0f)
            ? Mathf.Max(defaultHitStun, e.freezeFrameDuration)
            : defaultHitStun;

        StopAllCoroutines();
        StartCoroutine(StunFor(dur));
    }

    private IEnumerator StunFor(float dur)
    {
        IsStunned = true;
        CombatBus.Publish(new StunChangedEvent(myId, true));
        yield return new WaitForSeconds(dur);
        IsStunned = false;
        CombatBus.Publish(new StunChangedEvent(myId, false));
    }

    private bool IsBlockedAgainst(int attackerId)
    {
        BlockController bc = GetComponent<BlockController>();
        if (bc == null || !bc.IsBlocking) return false;

        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk)) return false;

        Vector3 dir = (atk.position - transform.position).normalized;
        dir.y = 0f;
        // positive dot ⇒ target is facing attacker
        return Vector3.Dot(transform.forward, dir) > 0.3f;
    }
}
