using UnityEngine;
using UnityEngine.Playables;

[ExecuteAlways]
[RequireComponent(typeof(Renderer))]
public class PlaneShakeDriver : MonoBehaviour
{
    [Header("Values you keyframe from Timeline")]
    [Range(0f, 3f)] public float Kick = 0f;
    
    public float Frequency = 9.5f;
    public float Amplitude = 0.85f;
    public float DecaySeconds = 4f;
    public float DecayCurve = 2f;

    [Header("IMPORTANT: Manually assign your Timeline!")]
    public PlayableDirector timeline;

    Renderer r;
    MaterialPropertyBlock mpb;

    static readonly int _KickID        = Shader.PropertyToID("_Kick");
    static readonly int _FreqID        = Shader.PropertyToID("_Frequency");
    static readonly int _AmpID         = Shader.PropertyToID("_Amplitude");
    static readonly int _DecaySecID    = Shader.PropertyToID("_DecaySeconds");
    static readonly int _DecayCurveID  = Shader.PropertyToID("_DecayCurve");
    static readonly int _CustomTimeID  = Shader.PropertyToID("_CustomTime");

    float runtimeKick = 0f;
    float lastAuthKick = -999f;
    float startTime = -1f;

    void OnEnable()
    {
        r = GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();
        
        // DON'T auto-find timeline - user must assign it manually!
        if (timeline == null)
        {
            Debug.LogWarning($"[{gameObject.name}] PlaneShakeDriver: Timeline not assigned! Please drag your Timeline into the Timeline field.", this);
        }
        else
        {
            Debug.Log($"[{gameObject.name}] PlaneShakeDriver enabled with timeline: {timeline.name}");
        }
        
        PushStaticParams();
    }

    void OnValidate()
    {
        if (r == null) r = GetComponent<Renderer>();
        if (mpb == null) mpb = new MaterialPropertyBlock();
        PushStaticParams();
    }

    void Update()
    {
        if (r == null || mpb == null) return;
        
        // Get current time from Timeline or runtime
        float currentTime = (timeline != null) ? (float)timeline.time : Time.time;
        
        // Detect when Timeline changes Kick value
        if (!Mathf.Approximately(Kick, lastAuthKick))
        {
            runtimeKick = Mathf.Max(0f, Kick);
            lastAuthKick = Kick;
            startTime = currentTime;
            Debug.Log($"[{gameObject.name}] Kick triggered! Value: {Kick}, Time: {currentTime}");
        }

        // Calculate decay over time
        float finalKick = 0f;
        float elapsedTime = 0f;
        
        if (runtimeKick > 0f && startTime >= 0f)
        {
            elapsedTime = currentTime - startTime;
            float t = elapsedTime / Mathf.Max(0.0001f, DecaySeconds);
            float decay = Mathf.Pow(Mathf.Clamp01(1f - t), Mathf.Max(0.0001f, DecayCurve));
            finalKick = runtimeKick * decay;
            
            if (decay <= 0f) 
            {
                runtimeKick = 0f;
            }
        }
        
        // Send to shader
        r.GetPropertyBlock(mpb);
        mpb.SetFloat(_KickID, finalKick);  // Decayed kick value
        mpb.SetFloat(_CustomTimeID, elapsedTime);  // Time for sine wave
        r.SetPropertyBlock(mpb);
    }

    void PushStaticParams()
    {
        if (r == null) return;
        r.GetPropertyBlock(mpb);
        mpb.SetFloat(_FreqID, Frequency);
        mpb.SetFloat(_AmpID, Amplitude);
        mpb.SetFloat(_DecaySecID, DecaySeconds);
        mpb.SetFloat(_DecayCurveID, DecayCurve);
        r.SetPropertyBlock(mpb);
    }
}