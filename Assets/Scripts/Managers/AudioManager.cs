using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static AudioManager;

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
        // Music
        GameMusic,
        MenuMusic,

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
        EndDragging

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
        EventBus.OnGameOpen += HandleGameStart;
        EventBus.OnMenuOpen += HandleMenuOpen;
    }

    void UnsubscribeDependencies()
    {
        EventBus.OnGameOpen -= HandleGameStart;
        EventBus.OnMenuOpen -= HandleMenuOpen;
    }

    private void InitializeSoundTypeMapping()
    {
        // Music
        soundTypeToMixerGroup[SoundType.GameMusic] = musicMixerGroup;
        soundTypeToMixerGroup[SoundType.MenuMusic] = musicMixerGroup;

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
    }

    private void InitializeAudioSources()
    {
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
    }

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

    public void StopAllSounds()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying)
            {
                sound.source.Stop();
            }
        }
    }

    public void StopAllSFX()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying &&
                soundTypeToMixerGroup[sound.type] == sfxMixerGroup)
            {
                sound.source.Stop();
            }
        }
    }

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

    #endregion

    #region Music Management

    public void PlayMusic(SoundType musicType)
    {
        if (IsPlaying(musicType)) return;

        StopAllMusic();
        PlaySound(musicType);
    }

    public void StopAllMusic()
    {
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying &&
                soundTypeToMixerGroup[sound.type] == musicMixerGroup)
            {
                sound.source.Stop();
            }
        }
    }

    public void CrossFadeMusic(SoundType newMusicType, float fadeDuration = 1f)
    {
        StartCoroutine(CrossFadeMusicCoroutine(newMusicType, fadeDuration));
    }

    private System.Collections.IEnumerator CrossFadeMusicCoroutine(SoundType newMusicType, float fadeDuration)
    {
        // FadeOut of current
        List<AudioSource> currentMusicSources = new List<AudioSource>();
        foreach (Sound sound in sounds)
        {
            if (sound.source != null && sound.source.isPlaying &&
                soundTypeToMixerGroup[sound.type] == musicMixerGroup)
            {
                currentMusicSources.Add(sound.source);
            }
        }

        // FadeIn of new
        Sound newMusic = sounds.Find(s => s.type == newMusicType);
        if (newMusic != null && newMusic.source != null)
        {
            newMusic.source.volume = 0f;
            newMusic.source.Play();

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / fadeDuration;

                foreach (AudioSource source in currentMusicSources)
                {
                    source.volume = Mathf.Lerp(source.volume, 0f, progress);
                }
                newMusic.source.volume = Mathf.Lerp(0f, newMusic.volume, progress);

                yield return null;
            }

            // Stop old
            foreach (AudioSource source in currentMusicSources)
            {
                source.Stop();
                source.volume = 1f;
            }

            newMusic.source.volume = newMusic.volume;
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

    #region Dependecies
    private void HandleGameStart()
    {
        PlayMusic(SoundType.GameMusic);
    }

    private void HandleMenuOpen(bool isMenuOpen)
    {
        if (isMenuOpen)
            PlayMusic(SoundType.MenuMusic);
        else
            PlayMusic(SoundType.GameMusic);
    }
    #endregion

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}