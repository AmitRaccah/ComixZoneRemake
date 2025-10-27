using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class HazardWarningUI : MonoBehaviour
{
    public static HazardWarningUI Instance;

    public WarningSlot left;
    public WarningSlot right;

    AudioSource audioSrc;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        audioSrc = GetComponent<AudioSource>();
        if (left?.image) left.image.enabled = false;
        if (right?.image) right.image.enabled = false;
    }

    public void Ping(HazardSide side, int flashesOverride = -1, float durationOverride = -1f, AudioClip sfxOverride = null)
    {
        if (side == HazardSide.Left) left?.Ping(this, audioSrc, flashesOverride, durationOverride, sfxOverride);
        else right?.Ping(this, audioSrc, flashesOverride, durationOverride, sfxOverride);
    }
}
