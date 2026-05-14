using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup masterMixerGroup;
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField] private AudioMixerGroup uiMixerGroup;
    [SerializeField] private AudioMixerGroup gameMixerGroup;

    [Header("Music Playlists")]
    [SerializeField] private MusicPlaylist menuMusicPlaylist;
    [SerializeField] private MusicPlaylist gameMusicPlaylist;

    private AudioSource menuMusicSource;
    private AudioSource gameMusicSource;
    private Coroutine currentMusicCoroutine = null;
    private bool isGameMusicPlaying = false;

    private const string MASTER_VOLUME_PARAM = "MasterVolume";
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";
    private const string UI_VOLUME_PARAM = "UIVolume";

    [System.Serializable]
    public class Sound
    {
        public SoundType type;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;

        [HideInInspector]
        public AudioSource source;
    }

    public enum SoundType
    {
        // Player
        PlayerDeath,
        PlayerHit,
        
        EnemyAttack,
        EnemyDeath,

        // UI
        ButtonClick,
        ButtonHover,
        MenuOpen,
        MenuClose,

        //Game
        GameRestart,
        PlayerWin,
        StartDragging,
        EndDragging,

        EndDay,
    }

    [SerializeField] private List<Sound> sounds = new List<Sound>();

    [Header("Global Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float uiVolume = 1f;

    private Dictionary<SoundType, AudioMixerGroup> soundTypeToMixerGroup = new Dictionary<SoundType, AudioMixerGroup>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicPlaylists();
            InitializeSoundTypeMapping();
            InitializeAudioSources();
            
        }
        else
        {
            Destroy(gameObject);
        }
        SubscribeDependencies();
        LoadAudioSettings();
    }

    private void Start()
    {

        UpdateAllVolumes();
        LoadAudioSettings();
    }

    void SubscribeDependencies()
    {
        EventBus.OnGameStart += HandleGameStart;
        EventBus.OnMenuOpen += HandleMenuOpen;
    }

    void UnsubscribeDependencies()
    {
        EventBus.OnGameStart -= HandleGameStart;
        EventBus.OnMenuOpen -= HandleMenuOpen;
    }

    #region Initialization
    private void InitializeSoundTypeMapping()
    {

        // SFX
        soundTypeToMixerGroup[SoundType.PlayerDeath] = sfxMixerGroup;
        soundTypeToMixerGroup[SoundType.PlayerHit] = sfxMixerGroup;

        soundTypeToMixerGroup[SoundType.EnemyAttack] = sfxMixerGroup;
        soundTypeToMixerGroup[SoundType.EnemyDeath] = sfxMixerGroup;

        //Game
        soundTypeToMixerGroup[SoundType.PlayerWin] = sfxMixerGroup;
        soundTypeToMixerGroup[SoundType.GameRestart] = sfxMixerGroup;

        // UI
        soundTypeToMixerGroup[SoundType.ButtonClick] = uiMixerGroup;
        soundTypeToMixerGroup[SoundType.ButtonHover] = uiMixerGroup;

        soundTypeToMixerGroup[SoundType.MenuOpen] = uiMixerGroup;
        soundTypeToMixerGroup[SoundType.MenuClose] = uiMixerGroup;
        
        soundTypeToMixerGroup[SoundType.EndDragging] = gameMixerGroup;
        soundTypeToMixerGroup[SoundType.StartDragging] = gameMixerGroup;

        soundTypeToMixerGroup[SoundType.EndDay] = gameMixerGroup;
    }

    private void InitializeAudioSources()
    {
        //Debug.Log("=== InitializeAudioSources START ===");
        var existingChildren = GetComponentsInChildren<AudioSource>(true);
        foreach (var child in existingChildren)
        {
            if (child.gameObject != gameObject)
            {
                //Debug.Log($"Destroying old AudioSource: {child.gameObject.name}");
                DestroyImmediate(child.gameObject);
            }
        }
        foreach (Sound sound in sounds)
        {
            sound.source = null;
        }

        foreach (Sound sound in sounds)
        {
            GameObject soundObject = new GameObject(sound.type.ToString());
            soundObject.transform.SetParent(transform);

            AudioSource audioSource = soundObject.AddComponent<AudioSource>();
            audioSource.clip = sound.clip;
            audioSource.volume = sound.volume;
            

            if (soundTypeToMixerGroup.ContainsKey(sound.type))
            {
                audioSource.outputAudioMixerGroup = soundTypeToMixerGroup[sound.type];
            }
            else
            {
                audioSource.outputAudioMixerGroup = sfxMixerGroup;
            }

            if (audioSource.outputAudioMixerGroup != null && audioSource.outputAudioMixerGroup.name == "Music")
            {

                audioSource.loop = true;
            }
            sound.source = audioSource;
        }

        if (menuMusicSource != null)
        {
            //Debug.Log("  Destroying old menuMusicSource");
            DestroyImmediate(menuMusicSource.gameObject);
        }
        if (gameMusicSource != null)
        {
            //Debug.Log("  Destroying old gameMusicSource");
            DestroyImmediate(gameMusicSource.gameObject);
        }

        menuMusicSource = CreateMusicSource("MenuMusicSource", menuMusicPlaylist);
        gameMusicSource = CreateMusicSource("GameMusicSource", gameMusicPlaylist);
       // Debug.Log("=== InitializeAudioSources END ===");
    }
 
    private AudioSource CreateMusicSource(string name, MusicPlaylist playlist)
    {
        GameObject musicObj = new GameObject(name);
        musicObj.transform.SetParent(transform);

        AudioSource source = musicObj.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = playlist != null ? playlist.targetMixerGroup : musicMixerGroup;
        source.loop = false;
        source.playOnAwake = false;

        return source;
    }
    public void InitializeMusicPlaylists()
    {
        if (menuMusicPlaylist != null)
        {
            menuMusicPlaylist.Initialize();
            //Debug.Log($"Menu playlist initialized with {menuMusicPlaylist.TrackCount} tracks");
        }

        if (gameMusicPlaylist != null)
        {
            gameMusicPlaylist.Initialize();
            //Debug.Log($"Game playlist initialized with {gameMusicPlaylist.TrackCount} tracks");
        }
    }
    #endregion

    #region Volume Control Methods

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    public void SetUIVolume(float volume)
    {
        uiVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    private void UpdateAllVolumes()
    {
        UpdateMixerVolume(MASTER_VOLUME_PARAM, masterVolume);
        UpdateMixerVolume(MUSIC_VOLUME_PARAM, musicVolume);
        UpdateMixerVolume(SFX_VOLUME_PARAM, sfxVolume);
        UpdateMixerVolume(UI_VOLUME_PARAM, uiVolume);
    }

    private void UpdateMixerVolume(string parameterName, float volume)
    {
        if (audioMixer != null)
        {
            float volumeDB = LinearToDecibels(volume);
            audioMixer.SetFloat(parameterName, volumeDB);
        }
    }

    private float LinearToDecibels(float linear)
    {
        return linear <= 0.0001f ? -80f : Mathf.Log10(linear) * 20f;
    }

    #endregion

    #region Sound Playback Methods

    public void PlaySound(SoundType soundType)
    {
        Sound sound = sounds.Find(s => s.type == soundType);
        if (sound != null && sound.source != null)
        {
            sound.source.Play();
        }
        else
        {
            Debug.LogWarning($"Sound {soundType} not found or not initialized!");
        }
    }

    public void StopSound(SoundType soundType)
    {
        Sound sound = sounds.Find(s => s.type == soundType);
        if (sound != null && sound.source != null)
        {
            sound.source.Stop();
        }
    }

    public void PlayOneShot(SoundType soundType)
    {
        Sound sound = sounds.Find(s => s.type == soundType);
        if (sound != null && sound.source != null)
        {
            sound.source.PlayOneShot(sound.clip);
        }
    }

    public void StopAllSounds(bool includeSFX = true, bool includeUI = true, bool includeGame = true)
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                bool shouldStop = false;
                var group = soundTypeToMixerGroup[sound.type];

                if (group == sfxMixerGroup && includeSFX) shouldStop = true;
                else if (group == uiMixerGroup && includeUI) shouldStop = true;
                else if (group == gameMixerGroup && includeGame) shouldStop = true;

                if (shouldStop) sound.source.Stop();
            }
        }
    }
    public void StopAllSFX() => StopAllSounds(true, false, false);

    public void PauseAllSounds()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Pause();
            }
        }
    }

    public void ResumeAllSounds()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && !sound.source.isPlaying)
            {
                sound.source.UnPause();
            }
        }
    }

    public void PlayRandomSoundFromFolder(string folderPath)
    {
        AudioClip[] clips = Resources.LoadAll<AudioClip>(folderPath);

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"No audio clips found in Resources/{folderPath}");
            return;
        }
        AudioClip randomClip = clips[Random.Range(0, clips.Length)];

        GameObject tempAudioSource = new GameObject($"TempAudio_{randomClip.name}");
        tempAudioSource.transform.SetParent(transform);

        AudioSource audioSource = tempAudioSource.AddComponent<AudioSource>();
        audioSource.clip = randomClip;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        audioSource.Play();
        Destroy(tempAudioSource, randomClip.length);
    }
    #endregion

    #region Music Management

    public void StopAllMusic()
    {
        if (currentMusicCoroutine != null)
        {
            StopCoroutine(currentMusicCoroutine);
            currentMusicCoroutine = null;
        }

        //Debug.Log("StopAllMusic called");

        if (menuMusicSource != null && menuMusicSource.isPlaying)
        {
            //Debug.Log("Stopping menuMusicSource");
            menuMusicSource.Stop();
        }

        if (gameMusicSource != null && gameMusicSource.isPlaying)
        {
            //Debug.Log("Stopping gameMusicSource");
            gameMusicSource.Stop();
        }
    }

    #endregion

    #region Save/Load Settings

    private void LoadAudioSettings()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        uiVolume = PlayerPrefs.GetFloat("UIVolume", 0.8f);
        UpdateAllVolumes();
    }

    private void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetFloat("UIVolume", uiVolume);
        PlayerPrefs.Save();
    }

    public void ResetToDefaultSettings()
    {
        masterVolume = 1f;
        musicVolume = 0.8f;
        sfxVolume = 0.8f;
        uiVolume = 0.8f;
        UpdateAllVolumes();
        SaveAudioSettings();
    }

    #endregion

    #region Utility Methods

    public bool IsPlaying(SoundType soundType)
    {
        Sound sound = sounds.Find(s => s.type == soundType);
        //return sound != null && sound.source != null && sound.source.isPlaying;
        if (sound != null && sound.source != null)
        {
            return sound.source.isPlaying;
        }
        return false;
    }
public float GetSoundLength(SoundType soundType)
    {
        Sound sound = sounds.Find(s => s.type == soundType);
        return sound != null && sound.clip != null ? sound.clip.length : 0f;
    }

    public AudioMixerGroup GetMixerGroupForSound(SoundType soundType)
    {
        return soundTypeToMixerGroup.ContainsKey(soundType) ? soundTypeToMixerGroup[soundType] : null;
    }

    #endregion

    #region playlist music
    private enum MusicType { Menu, Game }

    private void PlayMusic(MusicType type)
    {
        if (isRestoringMusic)
        {
            Debug.Log("PlayMusic skipped - restoring music after focus");
            return;
        }

        if (currentMusicCoroutine != null)
        {
            StopCoroutine(currentMusicCoroutine);
            currentMusicCoroutine = null;
        }

        StopAllMusic();

        MusicPlaylist playlist = type == MusicType.Menu ? menuMusicPlaylist : gameMusicPlaylist;
        AudioSource source = type == MusicType.Menu ? menuMusicSource : gameMusicSource;

        if (playlist == null || playlist.TrackCount == 0)
        {
            Debug.LogWarning($"No {(type == MusicType.Menu ? "menu" : "game")} music playlist or tracks!");
            return;
        }

        AudioClip nextTrack = playlist.GetNextTrack();
        if (nextTrack != null && source != null)
        {
            source.clip = nextTrack;
            source.Play();
            isGameMusicPlaying = (type == MusicType.Game);
            currentMusicCoroutine = StartCoroutine(WaitForMusicToEnd(source, playlist, type == MusicType.Game));
        }
    }

    public void PlayMenuMusic() => PlayMusic(MusicType.Menu);
    public void PlayGameMusic() => PlayMusic(MusicType.Game);

    private IEnumerator WaitForMusicToEnd(AudioSource source, MusicPlaylist playlist, bool isGame)
    {
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        if (currentMusicCoroutine == null) yield break;

        if (playlist != null && playlist.TrackCount > 0)
        {
            AudioClip nextTrack = playlist.GetNextTrack();
            if (nextTrack != null && source != null)
            {
                source.clip = nextTrack;
                source.Play();
                currentMusicCoroutine = StartCoroutine(WaitForMusicToEnd(source, playlist, isGame));
            }
        }
    }

    //public void SkipToNextMusicTrack()
    //{
    //    if (isGameMusicPlaying && gameMusicPlaylist != null)
    //    {
    //        gameMusicPlaylist.SkipToNextTrack();
    //        PlayGameMusic();
    //    }
    //    else if (!isGameMusicPlaying && menuMusicPlaylist != null)
    //    {
    //        menuMusicPlaylist.SkipToNextTrack();
    //        PlayMenuMusic();
    //    }
    //}

    //public void PlayNextGameMusicTrack()
    //{
    //    if (gameMusicPlaylist != null)
    //    {
    //        gameMusicPlaylist.SkipToNextTrack();
    //        PlayGameMusic();
    //    }
    //}

    //public void PlayNextMenuMusicTrack()
    //{
    //    if (menuMusicPlaylist != null)
    //    {
    //        menuMusicPlaylist.SkipToNextTrack();
    //        PlayMenuMusic();
    //    }
    //}


    #endregion

    #region Focus Handling

    private float savedMusicTime = 0f;
    private bool wasMusicPlaying = false;
    private bool isRestoringMusic = false;

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            AudioSource currentSource = isGameMusicPlaying ? gameMusicSource : menuMusicSource;
            if (currentSource != null && currentSource.isPlaying)
            {
                savedMusicTime = currentSource.time;
                wasMusicPlaying = true;
                currentSource.Stop();
                //Debug.Log($"Music saved at time: {savedMusicTime}");
            }
            else
            {
                wasMusicPlaying = false;
            }

            if (currentMusicCoroutine != null)
            {
                StopCoroutine(currentMusicCoroutine);
                currentMusicCoroutine = null;
            }
        }
        else if (wasMusicPlaying)
        {
            isRestoringMusic = true;
            AudioSource currentSource = isGameMusicPlaying ? gameMusicSource : menuMusicSource;
            MusicPlaylist playlist = isGameMusicPlaying ? gameMusicPlaylist : menuMusicPlaylist;

            if (currentSource != null)
            {
                currentSource.time = savedMusicTime;
                currentSource.Play();
                currentMusicCoroutine = StartCoroutine(WaitForMusicToEnd(currentSource, playlist, isGameMusicPlaying));
                //Debug.Log($"Music resumed from time: {savedMusicTime}");
            }
            isRestoringMusic = false;
        }
    }

    #endregion

    #region Dependecies
    private void HandleGameStart()
    {
        PlayGameMusic();
    }

    private void HandleMenuOpen(bool isMenuOpen)
    {
        if (isMenuOpen)
            PlayMenuMusic();
        else
            PlayGameMusic();
    }
    #endregion

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}