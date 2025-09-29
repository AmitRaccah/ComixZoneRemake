using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FactionId))]
[RequireComponent(typeof(Animator))]
public class DeathHandler : MonoBehaviour
{
    [Header("Death FX")]
    [SerializeField] private string deathTriggerName = "Death";

    [Header("Pooling")]
    [Tooltip("Extra delay before returning to pool (in addition to Health.DeathDelay).")]
    [SerializeField] private float poolReleaseDelay = 0f;

    [Tooltip("If object is not pooled, allow Destroy as a last resort. If false, just SetActive(false).")]
    [SerializeField] private bool allowDestroyIfNotPooled = false;

    private int myId;
    private FactionId faction;
    private Animator anim;
    private int deathHash;

    // Guards
    private bool isDying;
    private Coroutine pendingRelease;

    void Awake()
    {
        faction = GetComponent<FactionId>();
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
        deathHash = Animator.StringToHash(deathTriggerName);
    }

    void OnEnable()
    {
        isDying = false;
        CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
        if (pendingRelease != null)
        {
            StopCoroutine(pendingRelease);
            pendingRelease = null;
        }
    }

    void OnSpawnedFromPool()
    {
        isDying = false;
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

    void OnReturnedToPool()
    {
        isDying = false;
        if (pendingRelease != null)
        {
            StopCoroutine(pendingRelease);
            pendingRelease = null;
        }
    }

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;
        if (isDying) return; 

        isDying = true;

        bool isPlayer = CompareTag("Player") || (faction != null && faction.faction == Faction.Player);

        if (anim != null)
        {
            anim.SetTrigger(deathHash);
        }

        if (isPlayer)
        {
            CombatBus.Publish(new PlayerDownEvent(myId, e.killerId));
            return;
        }

        pendingRelease = StartCoroutine(ReleaseAfterDelays());
    }

    private IEnumerator ReleaseAfterDelays()
    {
        float delayFromHealth = 0f;
        Health h = GetComponent<Health>();
        if (h != null && h.DeathDelay > 0f)
        {
            delayFromHealth = h.DeathDelay;
        }

        float totalDelay = delayFromHealth + Mathf.Max(0f, poolReleaseDelay);
        if (totalDelay > 0f)
        {
            yield return new WaitForSeconds(totalDelay);
        }

        EnemyPoolMember pooled = GetComponent<EnemyPoolMember>();
        if (pooled != null && pooled.IsPooled)
        {
            pooled.Release(0f);
        }
        else
        {
            if (allowDestroyIfNotPooled)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
                Debug.LogWarning("DeathHandler: Object not pooled; deactivated instead of Destroy. Consider spawning via EnemyPool.");
            }
        }

        pendingRelease = null;
    }
}
