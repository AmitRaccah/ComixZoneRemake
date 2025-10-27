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

    public void Ping(HazardSide side, int flashes, float duration, AudioClip sfx)
    {
        if (sfx && audioSrc) audioSrc.PlayOneShot(sfx);

        if (side == HazardSide.Left)
            left?.Ping(this, flashes, duration);
        else
            right?.Ping(this, flashes, duration);
    }
}
