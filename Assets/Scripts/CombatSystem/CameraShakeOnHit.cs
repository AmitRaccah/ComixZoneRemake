using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShakeOnHit : MonoBehaviour
{
    private CinemachineImpulseSource _impulseSource;

    private void Awake()
    {
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null) ;
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

        float amplitude = e.attackData.shakeAmplitude;

        if (amplitude > 0f)
            _impulseSource.GenerateImpulse(Vector3.up * amplitude);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            _impulseSource.GenerateImpulse(Vector3.up * 1f);
        }
    }
}
