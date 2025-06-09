using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeOnHit : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulse;
    private int myId;

    private void Awake()
    {
        myId = gameObject.GetInstanceID();
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
        if (e.attackerId != myId)
        {
            return;
        }

        impulse.GenerateImpulse(Vector3.up * e.shakeAmplitude);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            impulse.GenerateImpulse(Vector3.up * 1f);
        }
    }
}
