using UnityEngine;

public class InkDripController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem dripParticles;
    [SerializeField] private Transform characterRoot;
    
    [Header("Drip Settings")]
    [SerializeField] private float dripRadius = 1.5f;
    [SerializeField] private int dripsPerSecond = 200;
    [SerializeField] private float dripSpeed = 2f;
    [SerializeField] private Vector2 dripSizeRange = new Vector2(0.05f, 0.15f);
    [SerializeField] private Color inkColor = Color.black;
    
    void Start()
    {
        // If no particle system assigned, create one
        if (dripParticles == null)
        {
            CreateDripParticleSystem();
        }
        
        // NOTE: Configure the particle system manually in the Inspector!
        // If you want automatic configuration, uncomment the line below:
        // ConfigureParticleSystem();
    }
    
    void CreateDripParticleSystem()
    {
        GameObject particleObj = new GameObject("InkDrips");
        particleObj.transform.SetParent(characterRoot != null ? characterRoot : transform);
        particleObj.transform.localPosition = Vector3.zero;
        
        dripParticles = particleObj.AddComponent<ParticleSystem>();
    }
    
    public void StartDrips(float yPosition)
    {
        if (dripParticles == null) return;
        
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
        if (dripParticles == null) return;
        
        Vector3 pos = dripParticles.transform.position;
        pos.y = yPosition;
        dripParticles.transform.position = pos;
    }
    
    public void StopEmission()
    {
        if (dripParticles == null) return;
        
        var emission = dripParticles.emission;
        emission.enabled = false;  // Stop spawning new drips, but existing ones keep falling
    }
    
    public void StopDrips()
    {
        if (dripParticles == null) return;
        
        if (dripParticles.isPlaying)
        {
            var emission = dripParticles.emission;
            emission.enabled = false;
            dripParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
    }
}