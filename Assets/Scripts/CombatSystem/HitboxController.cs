using UnityEngine;

public class HitboxController : MonoBehaviour
{

    private AttackData data;
    private Transform owner;
    private float timer;

    private bool armed = false;

    public void Init(AttackData d, Transform hand)
    {
        data = d;
        owner = hand;
        timer = d.activeTime;

        transform.localScale = Vector3.one * d.hitboxRadius;

        GetComponent<Collider>().enabled = false;
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

    void LateUpdate()               
    {
        transform.position = owner.TransformPoint(data.hitboxOffset);

        if (!armed)   
        {
            GetComponent<Collider>().enabled = true;
            armed = true;
        }

        timer -= Time.deltaTime;
        if (timer <= 0f) Destroy(gameObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!armed || other.transform == owner) return;

        Transform root = other.attachedRigidbody ?        
                         other.attachedRigidbody.transform :
                         other.transform.root;             

        CombatBus.Publish(new DamageEvent
        {
            attackerId = owner.GetInstanceID(),
            targetId = root.gameObject.GetInstanceID(),  
            amount = data.damage,
            knockback = data.knockback,
            type = data.damageType
        });
    }


}
