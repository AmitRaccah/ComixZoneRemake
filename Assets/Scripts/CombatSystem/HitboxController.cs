using UnityEngine;

public class HitboxController : MonoBehaviour
{

    private AttackData data;
    private Transform owner;
    private float timer;

    public void Init(AttackData d, Transform hand)
    {
        data = d;
        owner = hand;
        timer = d.activeTime;

        transform.localScale = Vector3.one * d.hitboxRadius;
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (timer < 0f) Destroy(gameObject);
        else transform.position = owner.TransformPoint(data.hitboxOffset);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == owner) return;

        CombatBus.Publish(new DamageEvent 
        {
            attackerId = owner.GetInstanceID(),
            targetId = other.GetInstanceID(),
            amount = data.damage,
            type = data.damageType
        });
    }


}
