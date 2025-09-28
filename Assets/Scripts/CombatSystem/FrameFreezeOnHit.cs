using UnityEngine;
using System.Collections;


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
            return;                                 

        float dur = (e.attackData != null)
                  ? e.attackData.GetFreezeFrameDuration(e.isBlocked)
                  : e.freezeFrameDuration;

        if (dur <= 0f) return;
        if (!isFreezing) StartCoroutine(Freeze(dur));
    }


    bool IsMe(int id)
    {
        if (id == gameObject.GetInstanceID()) return true;

        if (AttackActivator.TransformsById.TryGetValue(id, out var t))
            return t.root == transform;            

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
