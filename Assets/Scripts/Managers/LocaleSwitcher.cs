using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageSwitcher : MonoBehaviour
{
    [Header("Toggle References")]
    [SerializeField] private Toggle toggleEnglish;
    [SerializeField] private Toggle toggleRussian;

    private bool isInitialized = false;

    void Start()
    {
        StartCoroutine(InitializeAfterLocalization());

        toggleEnglish.onValueChanged.AddListener(OnEnglishToggled);
        toggleRussian.onValueChanged.AddListener(OnRussianToggled);
    }

    IEnumerator InitializeAfterLocalization()
    {
        yield return LocalizationSettings.InitializationOperation;

        isInitialized = true;

        UpdateToggleState();
    }

    void UpdateToggleState()
    {
        if (!isInitialized) return;

        string currentLocale = LocalizationSettings.SelectedLocale.Identifier.Code;

        toggleEnglish.SetIsOnWithoutNotify(currentLocale == "en");
        toggleRussian.SetIsOnWithoutNotify(currentLocale == "ru");

    }

    void OnEnglishToggled(bool isOn)
    {
        if (!isOn || !isInitialized) return;

        var englishLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
        if (englishLocale != null)
        {
            LocalizationSettings.SelectedLocale = englishLocale;
        }
    }

    void OnRussianToggled(bool isOn)
    {
        if (!isOn || !isInitialized) return;

        var russianLocale = LocalizationSettings.AvailableLocales.GetLocale("ru");
        if (russianLocale != null)
        {
            LocalizationSettings.SelectedLocale = russianLocale;
        }
    }

    void OnDestroy()
    {
        if (toggleEnglish != null)
            toggleEnglish.onValueChanged.RemoveListener(OnEnglishToggled);
        if (toggleRussian != null)
            toggleRussian.onValueChanged.RemoveListener(OnRussianToggled);
    }
}