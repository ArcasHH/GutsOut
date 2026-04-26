using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class AudioButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    private Button button;

    public enum ButtonAction
    {
        None,
        StartGame,
        OpenMenu,
        ExitGame,
        PauseGame,
        ContinueGame,
    }

    public ButtonAction action = ButtonAction.None;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            enabled = false;
            Debug.LogError($"AudioButton: Button component not found on {gameObject.name}", gameObject);
            return;
        }

        ColorBlock colors = button.colors;
        colors.highlightedColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
        colors.pressedColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
        button.colors = colors;
    }

    private void OnButtonClick()
    {
        Debug.Log($"[AudioButton] Clicked: {action} on {gameObject.name}");

        switch (action)
        {
            case ButtonAction.StartGame:
                Debug.Log("PlayGame pressed");
                GameSceneManager.Instance.StartGame();
                break;

            case ButtonAction.OpenMenu:
                Debug.Log("OpenMenu pressed");
                GameSceneManager.Instance.OpenMenu();
                break;

            case ButtonAction.ExitGame:
                Debug.Log("Exit pressed");
                GameSceneManager.Instance.ExitGame();
                break;

            case ButtonAction.PauseGame:
                EventBus.TriggerGamePaused(true);
                break;
            case ButtonAction.ContinueGame:
                EventBus.TriggerGamePaused(false);
                break;

            case ButtonAction.None:
            default:
                Debug.LogWarning($"[AudioButton] No action assigned for {gameObject.name}");
                break;
        }
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable)
            return;
        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonClick);
        OnButtonClick();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!button.interactable)
            return;
        AudioManager.Instance.PlaySound(AudioManager.SoundType.ButtonHover);
    }
}
