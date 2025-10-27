using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    [System.Serializable]
    public class MusicTrack
    {
        [Tooltip("The audio clip to play")]
        public AudioClip clip;
        
        [Tooltip("Start playing from this time (in seconds)")]
        public float startOffset = 0f;
        
        [Tooltip("Stop playing at this time (0 = play to end)")]
        public float endOffset = 0f;
        
        [Tooltip("Volume for this track (0-1)")]
        [Range(0f, 1f)]
        public float volume = 1f;
        
        [Tooltip("Should this track loop?")]
        public bool loop = true;
        
        [Tooltip("Fade in duration (in seconds)")]
        public float fadeInDuration = 2f;
        
        [Tooltip("Fade out duration (in seconds)")]
        public float fadeOutDuration = 2f;
    }
    
    [Header("Audio Sources")]
    public AudioSource musicSource1;
    public AudioSource musicSource2;
    
    [Header("Music Tracks")]
    public MusicTrack menuMusic = new MusicTrack();
    public MusicTrack transitionMusic = new MusicTrack();
    public MusicTrack gameMusic = new MusicTrack();
    
    [Header("Settings")]
    public bool playMenuMusicOnStart = true;
    public bool useCrossfade = true;
    [Tooltip("Time to wait before starting new track during crossfade")]
    public float crossfadeOffset = 0f;
    
    private AudioSource currentSource;
    private AudioSource nextSource;
    private Coroutine fadeCoroutine;
    private Coroutine trackEndCoroutine;
    
    public static MusicManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        SetupAudioSources();
    }
    
    void Start()
    {
        if (playMenuMusicOnStart && menuMusic.clip != null)
        {
            PlayMenuMusic();
        }
    }
    
    void SetupAudioSources()
    {
        if (musicSource1 == null)
        {
            GameObject source1 = new GameObject("MusicSource1");
            source1.transform.SetParent(transform);
            musicSource1 = source1.AddComponent<AudioSource>();
        }
        
        if (musicSource2 == null)
        {
            GameObject source2 = new GameObject("MusicSource2");
            source2.transform.SetParent(transform);
            musicSource2 = source2.AddComponent<AudioSource>();
        }
        
        ConfigureAudioSource(musicSource1);
        ConfigureAudioSource(musicSource2);
        
        currentSource = musicSource1;
        nextSource = musicSource2;
    }
    
    void ConfigureAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = 0f;
    }
    
    public void PlayMenuMusic()
    {
        PlayTrack(menuMusic);
    }
    
    public void PlayTransitionMusic()
    {
        PlayTrack(transitionMusic);
    }
    
    public void PlayTransitionMusic(float delay)
    {
        StartCoroutine(PlayTrackWithDelay(transitionMusic, delay));
    }
    
    public void PlayGameMusic()
    {
        PlayTrack(gameMusic);
    }
    
    public void PlayTrack(MusicTrack track)
    {
        if (track == null || track.clip == null)
        {
            Debug.LogWarning("MusicManager: Cannot play null track or clip");
            return;
        }
        
        // Validate the track before playing
        if (!ValidateTrack(track))
        {
            return;
        }
        
        if (useCrossfade && currentSource.isPlaying)
        {
            StartCoroutine(CrossfadeToTrack(track));
        }
        else
        {
            StartTrack(nextSource, track);
            SwapSources();
        }
    }
    
    bool ValidateTrack(MusicTrack track)
    {
        if (track.clip == null)
        {
            Debug.LogError("MusicManager: Track has no audio clip assigned!");
            return false;
        }
        
        float clipLength = track.clip.length;
        
        // Validate start offset
        if (track.startOffset < 0)
        {
            Debug.LogWarning($"MusicManager: Start offset ({track.startOffset}) is negative. Setting to 0.");
            track.startOffset = 0;
        }
        else if (track.startOffset >= clipLength)
        {
            Debug.LogError($"MusicManager: Start offset ({track.startOffset}s) is beyond clip length ({clipLength}s). Cannot play track '{track.clip.name}'.");
            return false;
        }
        
        // Validate end offset
        if (track.endOffset > 0)
        {
            if (track.endOffset > clipLength)
            {
                Debug.LogWarning($"MusicManager: End offset ({track.endOffset}s) exceeds clip length ({clipLength}s). Setting to clip length.");
                track.endOffset = clipLength;
            }
            
            if (track.endOffset <= track.startOffset)
            {
                Debug.LogError($"MusicManager: End offset ({track.endOffset}s) must be greater than start offset ({track.startOffset}s). Cannot play track '{track.clip.name}'.");
                return false;
            }
        }
        
        return true;
    }
    
    public void StopMusic(bool fadeOut = true, float fadeDuration = 2f)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (trackEndCoroutine != null)
        {
            StopCoroutine(trackEndCoroutine);
            trackEndCoroutine = null;
        }
        
        if (fadeOut)
        {
            fadeCoroutine = StartCoroutine(FadeOutAndStop(currentSource, fadeDuration));
        }
        else
        {
            currentSource.Stop();
            currentSource.volume = 0f;
        }
    }
    
    public void SetMasterVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);
        musicSource1.volume = Mathf.Min(musicSource1.volume, volume);
        musicSource2.volume = Mathf.Min(musicSource2.volume, volume);
    }
    
    void StartTrack(AudioSource source, MusicTrack track)
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        if (trackEndCoroutine != null)
        {
            StopCoroutine(trackEndCoroutine);
        }
        
        // Setup the audio source
        source.clip = track.clip;
        source.loop = track.loop;
        source.volume = 0f;
        
        // Start playing first
        source.Play();
        
        // THEN set the time offset (this prevents the seek error)
        if (track.startOffset > 0 && track.startOffset < track.clip.length)
        {
            source.time = Mathf.Clamp(track.startOffset, 0f, track.clip.length - 0.1f);
        }
        
        // Fade in
        fadeCoroutine = StartCoroutine(FadeIn(source, track.volume, track.fadeInDuration));
        
        // Setup end offset if specified
        if (track.endOffset > 0 && track.endOffset < track.clip.length)
        {
            float playDuration = track.endOffset - track.startOffset;
            trackEndCoroutine = StartCoroutine(StopAtEndOffset(source, playDuration, track.fadeOutDuration));
        }
    }
    
    IEnumerator CrossfadeToTrack(MusicTrack newTrack)
    {
        AudioSource fadeOutSource = currentSource;
        AudioSource fadeInSource = nextSource;
        
        float fadeOutDuration = newTrack.fadeOutDuration;
        Coroutine fadeOutCoroutine = StartCoroutine(FadeOut(fadeOutSource, fadeOutDuration));
        
        if (crossfadeOffset > 0)
        {
            yield return new WaitForSeconds(crossfadeOffset);
        }
        
        StartTrack(fadeInSource, newTrack);
        
        SwapSources();
        
        yield return fadeOutCoroutine;
        
        fadeOutSource.Stop();
    }
    
    IEnumerator FadeIn(AudioSource source, float targetVolume, float duration)
    {
        if (duration <= 0)
        {
            source.volume = targetVolume;
            yield break;
        }
        
        float elapsed = 0f;
        float startVolume = source.volume;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }
        
        source.volume = targetVolume;
    }
    
    IEnumerator FadeOut(AudioSource source, float duration)
    {
        if (duration <= 0)
        {
            source.volume = 0f;
            yield break;
        }
        
        float elapsed = 0f;
        float startVolume = source.volume;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            source.volume = Mathf.Lerp(startVolume, 0f, t);
            yield return null;
        }
        
        source.volume = 0f;
    }
    
    IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        yield return StartCoroutine(FadeOut(source, duration));
        source.Stop();
    }
    
    IEnumerator StopAtEndOffset(AudioSource source, float duration, float fadeOutDuration)
    {
        float fadeStartTime = duration - fadeOutDuration;
        if (fadeStartTime > 0)
        {
            yield return new WaitForSeconds(fadeStartTime);
        }
        
        yield return StartCoroutine(FadeOut(source, fadeOutDuration));
        
        source.Stop();
    }
    
    IEnumerator PlayTrackWithDelay(MusicTrack track, float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }
        
        PlayTrack(track);
    }
    
    void SwapSources()
    {
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }
    
    public MusicTrack GetMenuMusic()
    {
        return menuMusic;
    }
    
    public MusicTrack GetTransitionMusic()
    {
        return transitionMusic;
    }
    
    public MusicTrack GetGameMusic()
    {
        return gameMusic;
    }
    
    public bool IsPlaying()
    {
        return currentSource != null && currentSource.isPlaying;
    }
    
    public AudioClip GetCurrentClip()
    {
        if (currentSource != null)
        {
            return currentSource.clip;
        }
        return null;
    }
    
    public float GetCurrentTime()
    {
        if (currentSource != null)
        {
            return currentSource.time;
        }
        return 0f;
    }
}