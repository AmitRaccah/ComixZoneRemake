using System.Collections.Generic;
using UnityEngine;

public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private int poolSize = 12;
    [Range(0f, 1f)][SerializeField] private float spatialBlend = 0f;
    [SerializeField] private AudioRolloffMode rolloff = AudioRolloffMode.Logarithmic;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 20f;

    private readonly List<AudioSource> pool = new();
    private int nextIndex;

    void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var ch = new GameObject("SFX_Channel_" + i);
            ch.transform.SetParent(transform, false);
            var src = ch.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = spatialBlend;
            src.rolloffMode = rolloff;
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            pool.Add(src);
        }
    }

    AudioSource Next()
    {
        var src = pool[nextIndex];
        nextIndex = (nextIndex + 1) % pool.Count;
        return src;
    }

    public void Play(AudioCue cue, Vector3 position, Transform follow)
    {
        if (cue == null) return;
        var clip = cue.Pick();
        if (!clip) return;

        var src = Next();
        src.Stop();
        src.transform.SetParent(null);
        src.transform.position = position;
        src.clip = clip;
        src.volume = 1f;
        src.pitch = 1f;
        src.Play();

        if (follow != null && spatialBlend > 0f)
            StartCoroutine(FollowWhilePlaying(src, follow));
    }

    System.Collections.IEnumerator FollowWhilePlaying(AudioSource src, Transform follow)
    {
        while (src && src.isPlaying && follow)
        {
            src.transform.position = follow.position;
            yield return null;
        }
    }
}
