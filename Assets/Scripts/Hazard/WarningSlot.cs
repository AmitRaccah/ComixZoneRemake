using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class WarningSlot
{
    public Image image;

    Coroutine _co;

    public void Ping(MonoBehaviour host, int flashes, float duration)
    {
        if (!image) return;
        if (_co != null) host.StopCoroutine(_co);
        _co = host.StartCoroutine(Blink(flashes, duration));
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
