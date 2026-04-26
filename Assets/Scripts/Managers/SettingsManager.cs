using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Slider References")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    void Start()
    {
        InitializeSliders();
        LoadCurrentSettings();

        SubscribeToSliders();
    }

    
    private void InitializeSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = 1f;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = 1f;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = 1f;
        }

    }

    private void LoadCurrentSettings()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found!");
            return;
        }
        if (masterVolumeSlider != null)
            masterVolumeSlider.value = AudioManager.Instance.masterVolume;
        if (musicVolumeSlider != null)
            musicVolumeSlider.value = AudioManager.Instance.musicVolume;
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.value = AudioManager.Instance.sfxVolume;

    }

    private void SubscribeToSliders()
    {
        UnsubscribeFromSliders();

        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
    }

    private void UnsubscribeFromSliders()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveAllListeners();

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveAllListeners();

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    #region Slider Event Handlers

    private void OnMasterVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(value);
        }
    }

    private void OnUIVolumeChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetUIVolume(value);
        }
    }

    #endregion

    #region Text Display Methods


    #endregion

    #region Test Sound Methods

    private enum SoundTestType { Master, Music, SFX, UI }

    private bool IsSliderBeingDragged(Slider slider)
    {
        return slider.gameObject == UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
    }

    #endregion

    #region Public Methods

    public void ResetToDefaults()
    {
        masterVolumeSlider.value = 1f;
        musicVolumeSlider.value = 1f;
        sfxVolumeSlider.value = 1f;
    }

    public void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    #endregion

    private void OnDestroy()
    {
        UnsubscribeFromSliders();
    }

    private void OnEnable()
    {
        LoadCurrentSettings();
    }
   
}
