using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeOnHit : MonoBehaviour
{
    [SerializeField] CinemachineImpulseSource impulse;

    private void OnEnable()
    {
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }
    void OnDisable()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }


    void OnDamage(DamageEvent e)
    {
        int myId = gameObject.GetInstanceID();
        if (e.attackerId == myId)
            impulse.GenerateImpulse();
    }





    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            impulse.GenerateImpulse();
    }

}
