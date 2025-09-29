using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class VfxPoolMember : MonoBehaviour
{
    [Tooltip("A unique ID for this VFX, e.g., 'Small_Hit_Spark' or 'Block_Effect'.")]
    public string vfxId;

    public bool IsActive { get; private set; }

    private ParticleSystem mainParticleSystem;

    private void Awake()
    {
        mainParticleSystem = GetComponent<ParticleSystem>();

        var main = mainParticleSystem.main;
        main.stopAction = ParticleSystemStopAction.Callback;
    }

    public void PrepareForSpawn(Vector3 position, Quaternion rotation)
    {
        if (IsActive) return;

        gameObject.SetActive(true);
        transform.SetPositionAndRotation(position, rotation);
        mainParticleSystem.Play(true);
        IsActive = true;
    }

    public void ReturnToPool()
    {
        mainParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        IsActive = false;
        gameObject.SetActive(false);
    }

    private void OnParticleSystemStopped()
    {
        if (VfxPoolManager.Instance != null)
        {
            VfxPoolManager.Instance.Return(this);
        }
    }
}