using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LocalizedTMP : MonoBehaviour
{
    [Header("Localization")]
    public string translationKey = "";

    [Header("Font Spacing (Auto)")]
    public bool autoAdjustSpacing = true;

    [Header("Russian Spacing Settings")]
    private float russianSpacing = -42f;
    private float russianWordSpacing = 42f;
    private float defaultSpacing = 0f;
    private float defaultWordSpacing = 0f;

    private TextMeshProUGUI tmpText;
    private LocalizeStringEvent localizeEvent;

    private void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();

        SetupLocalization();
    }

    private void SetupLocalization()
    {
        if (string.IsNullOrEmpty(translationKey))
        {
            return;
        }

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

        LocalizationSettings.SelectedLocaleChanged += OnLocaleChanged;

        if (autoAdjustSpacing)
            AdjustCharacterSpacing();
    }

    private void OnTextUpdated(string localizedText)
    {
        if (tmpText != null && !string.IsNullOrEmpty(localizedText))
        {
            tmpText.text = localizedText;

            if (autoAdjustSpacing)
                AdjustCharacterSpacing();
        }
    }

    private void OnLocaleChanged(Locale newLocale)
    {
        if (autoAdjustSpacing)
            AdjustCharacterSpacing();
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

    public void RefreshTranslation()
    {
        if (localizeEvent != null)
            localizeEvent.RefreshString();
    }

    public void SetTranslationKey(string newKey)
    {
        translationKey = newKey;
        if (localizeEvent != null)
        {
            localizeEvent.StringReference.TableEntryReference = newKey;
            RefreshTranslation();
        }
    }

    private void OnDestroy()
    {
        if (localizeEvent != null)
            localizeEvent.OnUpdateString.RemoveListener(OnTextUpdated);

        LocalizationSettings.SelectedLocaleChanged -= OnLocaleChanged;
    }
}