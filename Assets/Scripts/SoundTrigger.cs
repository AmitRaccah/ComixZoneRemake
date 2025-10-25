using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class SoundTrigger : MonoBehaviour
{
    [SerializeField] private AudioClip clip;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (clip == null) return;

        source.Stop();
        source.clip = clip;
        source.Play();
    }
}
