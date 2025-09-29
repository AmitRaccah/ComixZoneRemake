using UnityEngine;
using System.Collections;

public class DeathHandler : MonoBehaviour
{
    [Header("Death FX")]
    [Tooltip("The name of the trigger parameter in the Animator to play the death animation.")]
    [SerializeField] private string deathTriggerName = "Death";

    [Header("Pooling")]
    [Tooltip("Delay before the object is returned to the pool, allowing time for the death animation to finish.")]
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
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
    }

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;

        HandleDeath();
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
            Debug.Log("Player Died. Game Over logic should be implemented here.");
            return;
        }

        StartCoroutine(ReleaseAfterDelays());
    }

    private IEnumerator ReleaseAfterDelays()
    {
        if (poolReleaseDelay > 0f)
        {
            yield return new WaitForSeconds(poolReleaseDelay);
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