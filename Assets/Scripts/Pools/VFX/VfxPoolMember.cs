using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VfxPoolMember : MonoBehaviour
{
    [Tooltip("A unique ID for this VFX, e.g., 'Small_Hit_Spark' or 'Block_Effect'.")]
    public string vfxId;

    public bool IsActive { get; private set; }

    private ParticleSystem[] allParticleSystems;
    private ParticleSystem mainParticleSystem;

    private void Awake()
    {
        allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);
        mainParticleSystem = GetComponent<ParticleSystem>();

        if (mainParticleSystem == null)
        {
            Debug.LogError("VfxPoolMember requires a ParticleSystem on the root object to function.", this);
            enabled = false;
            return;
        }

        var main = mainParticleSystem.main;
        if (main.stopAction != ParticleSystemStopAction.Callback)
        {
            main.stopAction = ParticleSystemStopAction.Callback;
        }
    }

    public void PrepareForSpawn(Vector3 position, Quaternion rotation)
    {
        if (IsActive) return;

        transform.SetPositionAndRotation(position, rotation);
        gameObject.SetActive(true);

        if (allParticleSystems == null || allParticleSystems.Length == 0)
            allParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < allParticleSystems.Length; i++)
            allParticleSystems[i].Play(true);

        IsActive = true;
    }

    public void ReturnToPool()
    {
        if (allParticleSystems != null)
        {
            for (int i = 0; i < allParticleSystems.Length; i++)
                allParticleSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        IsActive = false;
        gameObject.SetActive(false);
    }

    private void OnParticleSystemStopped()
    {
        if (IsActive && VfxPoolManager.Instance != null)
        {
            VfxPoolManager.Instance.Return(this);
        }
    }
}
