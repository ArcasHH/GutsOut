// DifficultySelector.cs
using UnityEngine;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    [Header("Toggles")]
    [SerializeField] private Toggle easyToggle;
    [SerializeField] private Toggle normalToggle;
    [SerializeField] private Toggle hardToggle;
    [SerializeField] private Toggle customToggle;

    private void Start()
    {
        easyToggle.onValueChanged.AddListener((isOn) => OnDifficultySelected(Difficulty.Easy, isOn));
        normalToggle.onValueChanged.AddListener((isOn) => OnDifficultySelected(Difficulty.Normal, isOn));
        hardToggle.onValueChanged.AddListener((isOn) => OnDifficultySelected(Difficulty.Hard, isOn));
        customToggle.onValueChanged.AddListener((isOn) => OnDifficultySelected(Difficulty.Custom, isOn));

        easyToggle.onValueChanged.AddListener((isOn) => changeDifficultSettingsEnable(false, isOn));
        normalToggle.onValueChanged.AddListener((isOn) => changeDifficultSettingsEnable(false, isOn));
        hardToggle.onValueChanged.AddListener((isOn) => changeDifficultSettingsEnable(false, isOn));
        customToggle.onValueChanged.AddListener((isOn) => changeDifficultSettingsEnable(true, isOn));

        LoadSavedDifficulty();
        SetColors();
    }

    private void SetColors()
    {
        if (ColorPaletteManager.Instance != null)
        {
            ColorBlock colors = easyToggle.colors;
            colors.highlightedColor = ColorPaletteManager.Instance.CurrentPalette.toggleHoverColor;
            colors.pressedColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
            easyToggle.colors = colors;
            normalToggle.colors = colors;
            hardToggle.colors = colors;
            customToggle.colors = colors;
        }
    }

    private void changeDifficultSettingsEnable(bool is_custom, bool isOn)
    {
        if (!isOn) return;
        if (is_custom)
        {
            EventBus.TriggerShowCustomDifficulty(true);
        }
        else
        {
            EventBus.TriggerShowCustomDifficulty(false);
        }
    }

    private void OnDifficultySelected(Difficulty difficulty, bool isOn)
    {
        if (isOn)
        {
            if (DataManager.Instance != null)
            {
                DataManager.Instance.SetDifficulty(difficulty);
            }
        }
    }

    private void LoadSavedDifficulty()
    {
        if (DataManager.Instance == null) return;

        Difficulty currentDifficulty = DataManager.Instance.GetCurrentDifficulty();

        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                easyToggle.isOn = true;
                break;
            case Difficulty.Normal:
                normalToggle.isOn = true;
                break;
            case Difficulty.Hard:
                hardToggle.isOn = true;
                break;
            case Difficulty.Custom:
                customToggle.isOn = true;
                break;
        }
    }
}