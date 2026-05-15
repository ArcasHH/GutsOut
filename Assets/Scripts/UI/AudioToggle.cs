using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class AudioToggle : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private Toggle toggle;
    private LocalizeStringEvent localizeEvent;
    private TextMeshProUGUI tmpText;

    [Header("Audio & Actions")]
    public ToggleAction action = ToggleAction.None;

    [Header("Localization (Optional)")]
    [Tooltip("Override default translation key. Leave empty to use auto-generated from action")]
    public string customTranslationKey = "";

    [Header("Custom Difficulty Settings")]
    [SerializeField] private GameObject customDifficultyPanel;

    private float russianSpacing = -42f;
    private float russianWordSpacing = 42f;
    private float defaultSpacing = 0f;
    private float defaultWordSpacing = 0f;

    public enum ToggleAction
    {
        None,
        SetEnglish,
        SetRussian,
        Easy,
        Normal,
        Hard,
        Custom,
    }

    void Start()
    {
        toggle = GetComponent<Toggle>();
        if (toggle == null)
        {
            enabled = false;
            return;
        }

        if (action == ToggleAction.SetEnglish || action == ToggleAction.SetRussian)
        {
            StartCoroutine(InitializeAfterLocalization());
        }
        else
        {
            LoadSavedState();
        }

        SetupLocalization();
        SetColors();

        toggle.onValueChanged.AddListener(OnToggleValueChanged);
    }

    private void LoadSavedState()
    {
        if (DataManager.Instance == null) return;
        Difficulty diff = DataManager.Instance.GetCurrentDifficulty();
        switch (action)
        {
            case ToggleAction.Easy:
                toggle.isOn = diff == Difficulty.Easy;
                break;
            case ToggleAction.Normal:
                toggle.isOn = diff == Difficulty.Normal;
                break;
            case ToggleAction.Hard:
                toggle.isOn = diff == Difficulty.Hard;
                break;
            case ToggleAction.Custom:
                toggle.isOn = diff == Difficulty.Custom;
                break;
        }
        SetDifficulty(diff);
    }

    IEnumerator InitializeAfterLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;
        UpdateToggleState();
    }

    private void UpdateToggleState()
    {
        if (LocalizationSettings.SelectedLocale == null) return;

        string currentLocale = LocalizationSettings.SelectedLocale.Identifier.Code;

        switch (action)
        {
            case ToggleAction.SetEnglish:
                toggle.SetIsOnWithoutNotify(currentLocale == "en");
                break;
            case ToggleAction.SetRussian:
                toggle.SetIsOnWithoutNotify(currentLocale == "ru");
                break;
        }
    }

    private void SetColors()
    {
        if (ColorPaletteManager.Instance != null)
        {
            ColorBlock colors = toggle.colors;
            colors.highlightedColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
            colors.pressedColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
            toggle.colors = colors;
        }
    }

    private void SetupLocalization()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText == null) return;

        string translationKey = GetTranslationKey();
        if (string.IsNullOrEmpty(translationKey)) return;

        localizeEvent = GetComponent<LocalizeStringEvent>();
        if (localizeEvent == null)
        {
            localizeEvent = gameObject.AddComponent<LocalizeStringEvent>();
        }

        localizeEvent.StringReference = new LocalizedString()
        {
            TableReference = "GutsOutLocalizationTable",
            TableEntryReference = translationKey
        };

        localizeEvent.OnUpdateString.AddListener(OnTextUpdated);
        localizeEvent.RefreshString();
    }

    private string GetTranslationKey()
    {
        if (!string.IsNullOrEmpty(customTranslationKey))
            return customTranslationKey;

        return action switch
        {
            ToggleAction.Easy => "easy",
            ToggleAction.Normal => "normal",
            ToggleAction.Hard => "hard",
            ToggleAction.Custom => "custom",
            ToggleAction.None => "",
            _ => ""
        };
    }

    private void OnTextUpdated(string localizedText)
    {
        if (tmpText != null && !string.IsNullOrEmpty(localizedText))
        {
            tmpText.text = localizedText;
            AdjustCharacterSpacing();
        }
    }

    private void AdjustCharacterSpacing()
    {
        if (tmpText == null) return;

        string currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (currentLanguage == "ru")
        {
            tmpText.characterSpacing = russianSpacing;
            tmpText.wordSpacing = russianWordSpacing;
        }
        else
        {
            tmpText.characterSpacing = defaultSpacing;
            tmpText.wordSpacing = defaultWordSpacing;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!toggle.interactable) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!toggle.interactable) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonHover);
    }

    private void OnToggleValueChanged(bool isOn)
    {
        if (!isOn) return;

        switch (action)
        {
            case ToggleAction.SetEnglish:
                SetEnglishLanguage();
                break;
            case ToggleAction.SetRussian:
                SetRussianLanguage();
                break;
            case ToggleAction.Easy:
                SetDifficulty(Difficulty.Easy);
                break;
            case ToggleAction.Normal:
                SetDifficulty(Difficulty.Normal);
                break;
            case ToggleAction.Hard:
                SetDifficulty(Difficulty.Hard);
                break;
            case ToggleAction.Custom:
                SetDifficulty(Difficulty.Custom);
                break;
        }
    }

    private void SetEnglishLanguage()
    {
        var englishLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
        if (englishLocale != null)
        {
            LocalizationSettings.SelectedLocale = englishLocale;
        }
    }

    private void SetRussianLanguage()
    {
        var russianLocale = LocalizationSettings.AvailableLocales.GetLocale("ru");
        if (russianLocale != null)
        {
            LocalizationSettings.SelectedLocale = russianLocale;
        }
    }

    private void SetDifficulty(Difficulty difficulty)
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SetDifficulty(difficulty);
        }
        bool isCustom = (difficulty == Difficulty.Custom);
        EventBus.TriggerShowCustomDifficulty(isCustom);
    }

    private void OnDestroy()
    {
        if (localizeEvent != null)
            localizeEvent.OnUpdateString.RemoveListener(OnTextUpdated);

        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
}