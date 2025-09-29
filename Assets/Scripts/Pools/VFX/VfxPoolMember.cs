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
            Debug.LogWarning($"The main particle system on {gameObject.name} does not have StopAction set to Callback. Auto-return to pool will not work.", this);
        }
    }

    public void PrepareForSpawn(Vector3 position, Quaternion rotation)
    {
        if (IsActive) return;

        gameObject.SetActive(true);
        transform.SetPositionAndRotation(position, rotation);

        // A more robust way to ensure all systems, including children, are reset and played.
        foreach (var ps in allParticleSystems)
        {
            // Clear any lingering particles and then play.
            ps.Clear(true);
            ps.Play(true);
        }

        IsActive = true;
    }

    public void ReturnToPool()
    {
        // This is not strictly necessary with SetActive(false), but it's good practice
        // to stop emitters before disabling the object.
        foreach (var ps in allParticleSystems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        IsActive = false;
        gameObject.SetActive(false);
    }

    // This is called by the main particle system when its duration is over.
    private void OnParticleSystemStopped()
    {
        if (VfxPoolManager.Instance != null)
        {
            VfxPoolManager.Instance.Return(this);
        }
    }
}