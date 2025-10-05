using UnityEngine;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float poolReleaseDelay = 0f;

    private Animator anim;
    private bool isDying = false;
    private int myId;

    void Awake()
    {
        anim = GetComponent<Animator>();
        myId = gameObject.GetInstanceID();
    }

    void OnEnable()
    {
        isDying = false;
        CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
        CoreBus.Subscribe<HealthChangedEvent>(OnHealthChanged); // NEW
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
        CoreBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged); // NEW
    }

    private void OnHealthChanged(HealthChangedEvent e) // NEW
    {
        if (e.entityId != myId) return;
        // אם חזרנו לחיים (HP>0), אפשר לשחרר את הנעילה
        if (isDying && !e.isDead && e.current > 0)
            isDying = false;
    }

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;
        HandleDeath(e.killerId);
    }

    public void HandleDeath(int killerId = -1)
    {
        if (isDying) return;
        isDying = true;

        if (anim != null)
            anim.SetTrigger(deathTriggerName);

        if (CompareTag("Player"))
        {
            CombatBus.Publish(new PlayerDownEvent(myId, killerId));
            return;
        }

        EnemyPoolMember pooled = GetComponent<EnemyPoolMember>();
        if (pooled != null && !string.IsNullOrEmpty(pooled.currentEncounterId))
            CoreBus.Publish(new EnemyDefeatedEvent(pooled.currentEncounterId));

        StartCoroutine(ReleaseAfterDelays());
    }

    private IEnumerator ReleaseAfterDelays()
    {
        if (poolReleaseDelay > 0f)
            yield return new WaitForSeconds(poolReleaseDelay);

        EnemyPoolMember pooled = GetComponent<EnemyPoolMember>();
        if (pooled != null) pooled.Release();
        else Destroy(gameObject);
    }
}
