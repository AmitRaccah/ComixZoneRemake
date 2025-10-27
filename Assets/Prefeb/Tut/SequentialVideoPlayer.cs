using UnityEngine;
using UnityEngine.Video;
using System.Collections;

/// <summary>
/// Plays multiple video clips in sequence
/// When one video ends, the next one starts automatically
/// </summary>
[RequireComponent(typeof(VideoPlayer))]
public class SequentialVideoPlayer : MonoBehaviour
{
    [Header("Video Playlist")]
    [Tooltip("Add your video clips in the order you want them to play")]
    public VideoClip[] videoClips;
    
    [Header("Playback Settings")]
    [Tooltip("Loop the entire playlist when it finishes")]
    public bool loopPlaylist = true;
    
    [Tooltip("Wait time between videos (seconds)")]
    [Range(0f, 5f)]
    public float delayBetweenVideos = 0f;
    
    [Tooltip("Start playing automatically")]
    public bool playOnStart = true;
    
    [Tooltip("Preload next video to reduce gray frame gap (recommended)")]
    public bool preloadNextVideo = true;
    
    [Tooltip("This setting is no longer used but kept for compatibility")]
    [Range(0.1f, 5f)]
    public float preloadTime = 1f;
    
    [Header("Debug")]
    [Tooltip("Show current video info in console")]
    public bool showDebugInfo = true;
    
    private VideoPlayer videoPlayer;
    private int currentVideoIndex = 0;
    private bool isPlaying = false;
    
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        
        // Subscribe to video end event
        videoPlayer.loopPointReached += OnVideoEnd;
        
        if (playOnStart && videoClips.Length > 0)
        {
            PlayPlaylist();
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"SequentialVideoPlayer: {videoClips.Length} videos loaded on {gameObject.name}");
        }
    }
    
    void OnVideoEnd(VideoPlayer vp)
    {
        if (showDebugInfo)
        {
            Debug.Log($"Video {currentVideoIndex + 1} finished: {videoClips[currentVideoIndex].name}");
        }
        
        // Move to next video
        currentVideoIndex++;
        
        // Check if we've reached the end
        if (currentVideoIndex >= videoClips.Length)
        {
            if (loopPlaylist)
            {
                if (showDebugInfo)
                {
                    Debug.Log("Playlist finished, looping back to start");
                }
                currentVideoIndex = 0;
                StartCoroutine(PlayNextVideoWithDelay());
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.Log("Playlist finished");
                }
                isPlaying = false;
            }
        }
        else
        {
            StartCoroutine(PlayNextVideoWithDelay());
        }
    }
    
    IEnumerator PlayNextVideoWithDelay()
    {
        if (delayBetweenVideos > 0)
        {
            yield return new WaitForSeconds(delayBetweenVideos);
        }
        
        // Prepare (preload) the video first
        if (preloadNextVideo && currentVideoIndex < videoClips.Length)
        {
            videoPlayer.clip = videoClips[currentVideoIndex];
            videoPlayer.Prepare();
            
            // Wait until video is prepared (loaded into memory)
            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }
            
            if (showDebugInfo)
            {
                Debug.Log($"Video {currentVideoIndex + 1} prepared and ready");
            }
        }
        
        PlayVideo(currentVideoIndex);
    }
    
    void PlayVideo(int index)
    {
        if (index < 0 || index >= videoClips.Length)
        {
            Debug.LogError($"SequentialVideoPlayer: Invalid video index {index}");
            return;
        }
        
        // Only set clip if not already prepared
        if (videoPlayer.clip != videoClips[index])
        {
            videoPlayer.clip = videoClips[index];
        }
        
        videoPlayer.Play();
        
        if (showDebugInfo)
        {
            Debug.Log($"Now playing video {index + 1}/{videoClips.Length}: {videoClips[index].name}");
        }
    }
    
    // Public methods to control playback
    
    /// <summary>
    /// Start playing the playlist from the beginning
    /// </summary>
    public void PlayPlaylist()
    {
        if (videoClips.Length == 0)
        {
            Debug.LogWarning("SequentialVideoPlayer: No video clips assigned!");
            return;
        }
        
        currentVideoIndex = 0;
        isPlaying = true;
        PlayVideo(0);
    }
    
    /// <summary>
    /// Stop playback
    /// </summary>
    public void StopPlaylist()
    {
        videoPlayer.Stop();
        isPlaying = false;
        
        if (showDebugInfo)
        {
            Debug.Log("Playlist stopped");
        }
    }
    
    /// <summary>
    /// Pause current video
    /// </summary>
    public void PausePlaylist()
    {
        videoPlayer.Pause();
        
        if (showDebugInfo)
        {
            Debug.Log("Playlist paused");
        }
    }
    
    /// <summary>
    /// Resume playback
    /// </summary>
    public void ResumePlaylist()
    {
        videoPlayer.Play();
        
        if (showDebugInfo)
        {
            Debug.Log("Playlist resumed");
        }
    }
    
    /// <summary>
    /// Skip to next video immediately
    /// </summary>
    public void SkipToNext()
    {
        currentVideoIndex++;
        
        if (currentVideoIndex >= videoClips.Length)
        {
            if (loopPlaylist)
            {
                currentVideoIndex = 0;
            }
            else
            {
                currentVideoIndex = videoClips.Length - 1;
                return;
            }
        }
        
        PlayVideo(currentVideoIndex);
    }
    
    /// <summary>
    /// Go back to previous video
    /// </summary>
    public void SkipToPrevious()
    {
        currentVideoIndex--;
        
        if (currentVideoIndex < 0)
        {
            if (loopPlaylist)
            {
                currentVideoIndex = videoClips.Length - 1;
            }
            else
            {
                currentVideoIndex = 0;
            }
        }
        
        PlayVideo(currentVideoIndex);
    }
    
    /// <summary>
    /// Jump to specific video in playlist
    /// </summary>
    public void PlayVideoAtIndex(int index)
    {
        if (index >= 0 && index < videoClips.Length)
        {
            currentVideoIndex = index;
            PlayVideo(currentVideoIndex);
        }
        else
        {
            Debug.LogWarning($"SequentialVideoPlayer: Index {index} out of range");
        }
    }
    
    /// <summary>
    /// Get current video index
    /// </summary>
    public int GetCurrentVideoIndex()
    {
        return currentVideoIndex;
    }
    
    /// <summary>
    /// Get total number of videos
    /// </summary>
    public int GetVideoCount()
    {
        return videoClips.Length;
    }
    
    /// <summary>
    /// Get current video name
    /// </summary>
    public string GetCurrentVideoName()
    {
        if (currentVideoIndex >= 0 && currentVideoIndex < videoClips.Length)
        {
            return videoClips[currentVideoIndex].name;
        }
        return "None";
    }
    
    void OnDestroy()
    {
        // Unsubscribe from event
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}