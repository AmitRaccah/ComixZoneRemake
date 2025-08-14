using UnityEngine;
using System.Collections;

/// Attach this script to every character (player & enemies).
/// It freezes only when:
///   • this object is the target,  OR
///   • the attackerId belongs to a component in this hierarchy.
public class FrameFreezeOnHit : MonoBehaviour
{
    private float storedFixedDelta;
    private bool isFreezing;

    void Awake() => storedFixedDelta = Time.fixedDeltaTime;

    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

    void OnDamage(DamageEvent e)
    {
        if (!IsMe(e.attackerId) && !IsMe(e.targetId))
            return;                                 // not related to me

        float dur = (e.attackData != null)
                  ? e.attackData.freezeFrameDuration
                  : e.freezeFrameDuration;

        if (dur <= 0f) return;
        if (!isFreezing) StartCoroutine(Freeze(dur));
    }

    /* -------------------------------------------------- */

    bool IsMe(int id)
    {
        if (id == gameObject.GetInstanceID()) return true;

        // attackerId belongs to AttackActivator (stored in dictionary)
        if (AttackActivator.TransformsById.TryGetValue(id, out var t))
            return t.root == transform;            // same hierarchy

        return false;
    }

    IEnumerator Freeze(float dur)
    {
        isFreezing = true;
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(dur);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = storedFixedDelta;
        isFreezing = false;
    }
}
