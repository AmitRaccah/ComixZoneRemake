using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AudioCue
{
    public List<AudioClip> clips = new List<AudioClip>();

    public AudioClip Pick()
    {
        if (clips == null || clips.Count == 0) return null;
        return clips[Random.Range(0, clips.Count)];
    }
}
