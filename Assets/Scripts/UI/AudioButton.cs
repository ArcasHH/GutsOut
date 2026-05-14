using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private Button button;
    private LocalizeStringEvent localizeEvent;
    private TextMeshProUGUI tmpText;

    [Header("Audio & Actions")]
    public ButtonAction action = ButtonAction.None;

    [Header("Localization (Optional)")]
    [Tooltip("Override default translation key. Leave empty to use auto-generated from action")]
    public string customTranslationKey = "";

    private float russianSpacing = -42f;
    private float russianWordSpacing = 42f;
    private float defaultSpacing = 0f;
    private float defaultWordSpacing = 0f;
    public enum ButtonAction
    {
        None,
        StartGame,
        OpenMenu,
        ExitGame,
        PauseGame,
        ContinueGame,
        NextTrack,
    }

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            enabled = false;
            //Debug.LogError($"AudioButton: Button component not found on {gameObject.name}", gameObject);
            return;
        }

        if (ColorPaletteManager.Instance != null)
        {
            ColorBlock colors = button.colors;
            colors.highlightedColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
            colors.pressedColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
            button.colors = colors;
        }

        SetupLocalization();
    }

    private void SetupLocalization()
    {
        tmpText = GetComponentInChildren<TextMeshProUGUI>();

        if (tmpText == null)
        {
            return;
        }

        string translationKey = GetTranslationKey();

        if (string.IsNullOrEmpty(translationKey))
        {
            //Debug.LogWarning($"[AudioButton] No translation key for action '{action}' on {gameObject.name}", gameObject);
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

        //Debug.Log($"[AudioButton] Localized '{action}' → '{translationKey}' on {gameObject.name}", gameObject);
    }

    private string GetTranslationKey()
    {
        if (!string.IsNullOrEmpty(customTranslationKey))
            return customTranslationKey;

        return action switch
        {
            ButtonAction.StartGame => "play_button",
            ButtonAction.OpenMenu => "menu_button",
            ButtonAction.ExitGame => "exit_button",
            ButtonAction.PauseGame => "pause_button",
            ButtonAction.ContinueGame => "play_button",
            ButtonAction.NextTrack => "next_track_button",
            ButtonAction.None => "",
            _ => ""
        };
    }

    private void OnTextUpdated(string localizedText)
    {
        if (tmpText != null && !string.IsNullOrEmpty(localizedText))
        {
            tmpText.text = localizedText;
            AdjustCharacterSpacing();
            //Debug.Log($"[AudioButton] '{action}' localized to: {localizedText}", gameObject);
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
        if (!button.interactable)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick);

        OnButtonClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonHover);
    }

    private void OnButtonClick()
    {
        //Debug.Log($"[AudioButton] Action: {action} on {gameObject.name}");

        switch (action)
        {
            case ButtonAction.StartGame:
                if (GameSceneManager.Instance != null)
                    GameSceneManager.Instance.StartGame();
                break;

            case ButtonAction.OpenMenu:
                if (GameSceneManager.Instance != null)
                    GameSceneManager.Instance.OpenMenu();
                break;

            case ButtonAction.ExitGame:
                if (GameSceneManager.Instance != null)
                    GameSceneManager.Instance.ExitGame();
                break;

            case ButtonAction.PauseGame:
                EventBus.TriggerGamePaused(true);
                break;

            case ButtonAction.ContinueGame:
                EventBus.TriggerGamePaused(false);
                break;

            case ButtonAction.NextTrack:
                EventBus.TriggerNextMusicTrack();
                break;

            case ButtonAction.None:
            default:
                //Debug.LogWarning($"[AudioButton] No action assigned for {gameObject.name}");
                break;
        }
    }

    private void OnDestroy()
    {
        if (localizeEvent != null)
            localizeEvent.OnUpdateString.RemoveListener(OnTextUpdated);
    }
}