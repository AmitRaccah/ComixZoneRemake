using UnityEngine;

public class KickReceiver : MonoBehaviour
{
    Material mat;
    int kickID;

    [Range(0f, 3f)] public float defaultAmp = 1f;

    void Awake()
    {
   
        var r = GetComponent<Renderer>();
        if (r != null) { r.material = new Material(r.sharedMaterial); mat = r.material; }
        kickID = Shader.PropertyToID("_Kick");
    }

  
    public void Kick(float amp)
    {
        if (mat == null) return;
        mat.SetFloat(kickID, amp > 0f ? amp : defaultAmp);
    }
}