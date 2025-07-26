using UnityEngine;
using System.Collections;

public class FrameFreezeOnHit : MonoBehaviour
{
    private int myId;
    private float originalFixedDeltaTime;

    private void Awake()
    {
        myId = gameObject.GetInstanceID();
        originalFixedDeltaTime = Time.fixedDeltaTime;
    }

    private void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void OnDamage(DamageEvent e)
    {
        if (e.attackerId == myId || e.targetId == myId)
        {
            StartCoroutine(FreezeFrame(e.freezeFrameDuration));
        }
    }

    private IEnumerator FreezeFrame(float duration)
    {
        Time.timeScale = 0f;
        Time.fixedDeltaTime = originalFixedDeltaTime * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = originalFixedDeltaTime;
    }
}
