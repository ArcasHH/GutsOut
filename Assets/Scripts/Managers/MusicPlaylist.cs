using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "MusicPlaylist", menuName = "Audio/Music Playlist")]
public class MusicPlaylist : ScriptableObject
{
    [Header("Track Sources")]
    public TrackSourceMode trackSourceMode = TrackSourceMode.ManualAssignment;

    [Header("Manual Assignment")]
    public List<AudioClip> manualTracks = new List<AudioClip>();

    [Header("Folder Loading (Resources folder)")]
    public string resourcesFolderPath = "Music/GameMusic";

    [Header("Playback Settings")]
    public PlaybackMode mode = PlaybackMode.Sequential;
    public bool startFromRandomTrack = true;
    public bool loopPlaylist = true;

    [Header("Mixer Settings")]
    public AudioMixerGroup targetMixerGroup;

    public enum TrackSourceMode
    {
        ManualAssignment,
        LoadFromResources
    }

    public enum PlaybackMode
    {
        Sequential,
        Random  
    }

    private List<AudioClip> runtimeTracks = new List<AudioClip>();
    private int currentTrackIndex = -1;
    private int lastPlayedIndex = -1;

    public int TrackCount => runtimeTracks.Count;
    public AudioClip CurrentTrack => GetTrackAtIndex(currentTrackIndex);

    /// <summary>
    /// Initialize the playlist
    /// </summary>
    public void Initialize()
    {
        LoadTracks();
        SetupRandomStart();
    }

    private void LoadTracks()
    {
        runtimeTracks.Clear();

        switch (trackSourceMode)
        {
            case TrackSourceMode.ManualAssignment:
                runtimeTracks.AddRange(manualTracks);
                break;

            case TrackSourceMode.LoadFromResources:
                AudioClip[] loadedClips = Resources.LoadAll<AudioClip>(resourcesFolderPath);
                runtimeTracks.AddRange(loadedClips);
                //Debug.Log($"Loaded {runtimeTracks.Count} tracks from Resources/{resourcesFolderPath}");
                break;
        }

        if (runtimeTracks.Count == 0)
        {
            Debug.LogWarning($"Playlist has no tracks! Mode: {trackSourceMode}");
        }
    }

    private void SetupRandomStart()
    {
        if (!startFromRandomTrack || runtimeTracks.Count <= 1) return;

        currentTrackIndex = Random.Range(0, runtimeTracks.Count);
        lastPlayedIndex = currentTrackIndex;
    }

    private AudioClip GetTrackAtIndex(int index)
    {
        if (runtimeTracks.Count == 0) return null;
        if (index < 0 || index >= runtimeTracks.Count) return null;
        return runtimeTracks[index];
    }

    /// <summary>
    /// Get the next track to play
    /// </summary>
    public AudioClip GetNextTrack()
    {
        if (runtimeTracks.Count == 0) return null;

        int newIndex = currentTrackIndex;

        switch (mode)
        {
            case PlaybackMode.Sequential:
                newIndex = GetNextSequentialIndex();
                break;

            case PlaybackMode.Random:
                newIndex = GetNextRandomIndex();
                break;
        }

        currentTrackIndex = newIndex;
        lastPlayedIndex = currentTrackIndex;

        return runtimeTracks[currentTrackIndex];
    }

    private int GetNextSequentialIndex()
    {
        if (currentTrackIndex == -1)
        {
            return startFromRandomTrack ? Random.Range(0, runtimeTracks.Count) : 0;
        }

        int nextIndex = currentTrackIndex + 1;

        if (nextIndex >= runtimeTracks.Count)
        {
            return loopPlaylist ? 0 : currentTrackIndex;
        }

        return nextIndex;
    }

    private int GetNextRandomIndex()
    {
        if (runtimeTracks.Count == 1) return 0;
        if (currentTrackIndex == -1)
        {
            return Random.Range(0, runtimeTracks.Count);
        }

        int newIndex;
        do
        {
            newIndex = Random.Range(0, runtimeTracks.Count);
        } while (newIndex == currentTrackIndex);

        return newIndex;
    }

    /// <summary>
    ///Switch to the next track (without playback)
    /// </summary>
    public void SkipToNextTrack()
    {
        if (runtimeTracks.Count == 0) return;
        currentTrackIndex = GetNextSequentialIndex();
        lastPlayedIndex = currentTrackIndex;
    }

    /// <summary>
    /// Reset the playlist (start over)
    /// </summary>
    public void ResetPlaylist()
    {
        currentTrackIndex = -1;
        lastPlayedIndex = -1;
        SetupRandomStart();
    }

    /// <summary>
    /// Get the current index (for debugging)
    /// </summary>
    public int GetCurrentIndex() => currentTrackIndex;

    /// <summary>
    /// Get the name of the current track
    /// </summary>
    public string GetCurrentTrackName()
    {
        return CurrentTrack != null ? CurrentTrack.name : "None";
    }
}