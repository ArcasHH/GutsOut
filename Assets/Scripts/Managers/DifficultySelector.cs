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

        LoadSavedDifficulty();
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