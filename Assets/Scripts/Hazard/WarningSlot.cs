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

    public void Ping(MonoBehaviour host, AudioSource audio, int flashesOverride = -1, float durationOverride = -1f, AudioClip sfxOverride = null)
    {
        if (!image) return;
        if (_co != null) host.StopCoroutine(_co);
        var clip = sfxOverride ? sfxOverride : sfx;
        if (clip && audio) audio.PlayOneShot(clip);

        int flashesToUse = flashesOverride > 0 ? flashesOverride : Mathf.Max(1, flashes);
        float durationToUse = durationOverride >= 0f ? Mathf.Max(0f, durationOverride) : Mathf.Max(0f, duration);

        _co = host.StartCoroutine(Blink(flashesToUse, durationToUse));
    }

    IEnumerator Blink(int flashesCount, float totalDuration)
    {
        int n = Mathf.Max(1, flashesCount);
        float d = Mathf.Max(0f, totalDuration);
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