using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FactionId))]
[RequireComponent(typeof(Animator))]
public class DeathHandler : MonoBehaviour
{
    [Header("Death FX")]
    [SerializeField] private string deathTriggerName = "Death";
    [SerializeField] private float removeDelay = 2f;
    [SerializeField] private Collider[] extraCollidersToDisable;

    private int myId; private FactionId faction; private Animator anim; private int deathHash;
    private Rigidbody rb; private CharacterController cc;

    void Awake()
    {
        faction = GetComponent<FactionId>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        myId = faction ? faction.EntityId : gameObject.GetInstanceID();
        deathHash = Animator.StringToHash(deathTriggerName);
    }

    void OnEnable() => CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
    void OnDisable() => CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);

    private void OnDead(HealthDepletedEvent e)
    {
        if (e.entityId != myId) return;

        anim.SetTrigger(deathHash);
        DisableCollisionsAndControl();

        if (faction.faction == Faction.Player) CombatBus.Publish(new PlayerDownEvent(myId, e.killerId));
        else if (faction.faction == Faction.Enemy) CombatBus.Publish(new EnemyDownEvent(myId, e.killerId));

        StartCoroutine(RemoveAfterDelay());
    }

    private void DisableCollisionsAndControl()
    {
        if (rb) rb.isKinematic = true;
        if (cc) cc.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>()) col.enabled = false;
        if (extraCollidersToDisable != null)
            foreach (var c in extraCollidersToDisable) if (c) c.enabled = false;

        var tpc = GetComponent<StarterAssets.ThirdPersonController>();
        if (tpc) tpc.enabled = false;
    }

    private IEnumerator RemoveAfterDelay()
    {
        yield return new WaitForSeconds(removeDelay);
        Destroy(gameObject);
    }
}
