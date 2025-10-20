using UnityEngine;

public class InkDripController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem dripParticles;
    [SerializeField] private Transform characterRoot;
    
    [Header("Drip Settings")]
    [SerializeField] private float dripRadius = 0.3f;
    [SerializeField] private int dripsPerSecond = 100;
    [SerializeField] private float dripSpeed = 2f;
    [SerializeField] private Vector2 dripSizeRange = new Vector2(0.0003f, 0.004f);
    [SerializeField] private Color inkColor = Color.black;
    
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shapeModule;
    
    void Start()
    {
        if (dripParticles != null)
        {
            ConfigureParticleSystem();
        }
    }
    
    void ConfigureParticleSystem()
    {
        mainModule = dripParticles.main;
        emission = dripParticles.emission;
        shapeModule = dripParticles.shape;
    
        mainModule.loop = false;
        mainModule.playOnAwake = false;
        mainModule.startLifetime = new ParticleSystem.MinMaxCurve(3f, 5f);  // MUCH LONGER lifetime
        mainModule.startSpeed = 0f;  // No initial speed
        mainModule.startSize = new ParticleSystem.MinMaxCurve(dripSizeRange.x, dripSizeRange.y);
        mainModule.startColor = inkColor;
        mainModule.gravityModifier = 0.3f;  // VERY SLOW gravity (was 1.5-2f)
        mainModule.maxParticles = 500;
        mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
    
        emission.rateOverTime = dripsPerSecond;
    
        shapeModule.shapeType = ParticleSystemShapeType.Circle;
        shapeModule.radius = dripRadius;
        shapeModule.radiusThickness = 1f;
    }
    
    public void StartDrips(float yPosition)
    {
        Vector3 pos = dripParticles.transform.position;
        pos.y = yPosition;
        dripParticles.transform.position = pos;
        
        var emission = dripParticles.emission;
        emission.enabled = true;
        
        if (!dripParticles.isPlaying)
        {
            dripParticles.Play();
        }
    }
    
    public void UpdateDripPosition(float yPosition)
    {
        Vector3 pos = dripParticles.transform.position;
        pos.y = yPosition;
        dripParticles.transform.position = pos;
    }
    
    public void StopEmission()
    {
        var emission = dripParticles.emission;
        emission.enabled = false;
    }
    
    public void StopDrips()
    {
        if (dripParticles != null && dripParticles.isPlaying)
        {
            var emission = dripParticles.emission;
            emission.enabled = false;
            dripParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}