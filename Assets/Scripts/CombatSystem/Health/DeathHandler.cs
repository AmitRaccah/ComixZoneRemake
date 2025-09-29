using UnityEngine;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [Header("Death FX")]
    [SerializeField] private string deathTriggerName = "Death";

    [Header("Pooling")]
    [Tooltip("השהייה נוספת לפני שהאויב חוזר לבריכה (בנוסף להשהייה מסקריפט ה-Health).")]
    [SerializeField] private float poolReleaseDelay = 0f;

    private Animator anim;
    private bool isDying = false;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void OnEnable()
    {
        isDying = false;
        // CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
    }

    void OnDisable()
    {
        // CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
    }

    public void HandleDeath()
    {
        if (isDying) return;
        isDying = true;

        if (anim != null)
        {
            anim.SetTrigger(deathTriggerName);
        }

        if (CompareTag("Player"))
        {
            return;
        }

        StartCoroutine(ReleaseAfterDelays());
    }

    private IEnumerator ReleaseAfterDelays()
    {
        float totalDelay = poolReleaseDelay;


        if (totalDelay > 0f)
        {
            yield return new WaitForSeconds(totalDelay);
        }

        EnemyPoolMember pooled = GetComponent<EnemyPoolMember>();
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}