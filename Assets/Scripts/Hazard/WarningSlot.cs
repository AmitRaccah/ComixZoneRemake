using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class WarningSlot
{
    public Image image;           
    public AudioClip sfx;        
    [Min(1)] public int flashes = 3;
    [Min(0f)] public float duration = 0.8f;

    Coroutine _co;

    public void Ping(MonoBehaviour host, AudioSource audio)
    {
        if (!image) return;
        if (_co != null) host.StopCoroutine(_co);
        if (sfx && audio) audio.PlayOneShot(sfx);
        _co = host.StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        int n = Mathf.Max(1, flashes);
        float d = Mathf.Max(0f, duration);
        float half = n > 0 ? d / (n * 2f) : 0f;

        image.enabled = false;
        for (int i = 0; i < n; i++)
        {
            image.enabled = true;
            if (half > 0f) yield return new WaitForSeconds(half);
            image.enabled = false;
            if (half > 0f) yield return new WaitForSeconds(half);
        }
        image.enabled = false; 
    }
}
