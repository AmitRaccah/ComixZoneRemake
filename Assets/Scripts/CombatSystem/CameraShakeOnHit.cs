using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShakeOnHit : MonoBehaviour
{
    private CinemachineImpulseSource _impulseSource;
    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
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
        float amplitude = e.shakeAmplitude; 
        if (e.attackData != null)
        {
            amplitude = e.attackData.shakeAmplitude; 
        }
        if (amplitude > 0f)
            _impulseSource.GenerateImpulse(Vector3.up * amplitude);
    }
}