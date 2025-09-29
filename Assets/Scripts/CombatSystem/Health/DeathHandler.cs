using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FactionId))]
[RequireComponent(typeof(Animator))]
public class DeathHandler : MonoBehaviour
{
    [Header("Death FX")]
    [SerializeField] private string deathTriggerName = "Death";

    [Header("Pooling")]
    [Tooltip("Additional delay before the enemy is returned to the pool after dying.")]
    [SerializeField] private float poolReleaseDelay = 0f;

    private int myId;
    private FactionId faction;
    private Animator anim;
    private int deathHash;

    void Awake()
    {
        faction = GetComponent<FactionId>();
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();   
        deathHash = Animator.StringToHash(deathTriggerName);
    }


    void OnEnable()
    {
        CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
    }

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;

        bool isPlayer = CompareTag("Player") || (faction != null && faction.faction == Faction.Player);

        if (anim != null) anim.SetTrigger(deathHash);

        if (isPlayer)
        {
            CombatBus.Publish(new PlayerDownEvent(myId, e.killerId));
            return;
        }

        StartCoroutine(RemoveAfterDelayFromHealth());
    }

    private IEnumerator RemoveAfterDelayFromHealth()
    {
        Health h = GetComponent<Health>();
        float delay = h != null ? h.DeathDelay : 0f;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        EnemyPoolMember pooled = GetComponent<EnemyPoolMember>();
        if (pooled != null && pooled.IsPooled)
        {
            float totalDelay = Mathf.Max(0f, poolReleaseDelay);
            pooled.Release(totalDelay);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
