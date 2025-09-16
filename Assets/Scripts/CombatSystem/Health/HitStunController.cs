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
}
public struct StunChangedEvent { public int entityId; public bool isStunned; public StunChangedEvent(int id, bool s) { entityId = id; isStunned = s; } }
