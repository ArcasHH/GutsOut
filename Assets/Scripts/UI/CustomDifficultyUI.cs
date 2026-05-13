using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomDifficultyUI : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private Slider knifeBaseCostSlider;
    [SerializeField] private TMP_Text knifeBaseCostValue;
    [SerializeField] private Slider knifeCostIncreaseSlider;
    [SerializeField] private TMP_Text knifeCostIncreaseValue;

    [Header("UI References")]
    [SerializeField] private GameObject customSettingsPanel;

    [Header("Preview")]
    [SerializeField] private TMP_Text previewText;

    private GameBalance customBalance;

    private void Start()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDifficultyChanged += OnDifficultyChanged;
            customBalance = DataManager.Instance.GetCustomBalance();

            if (knifeBaseCostSlider != null)
            {
                knifeBaseCostSlider.onValueChanged.AddListener(OnKnifeBaseCostChanged);
                knifeBaseCostSlider.wholeNumbers = true;
            }

            if (knifeCostIncreaseSlider != null)
            {
                knifeCostIncreaseSlider.onValueChanged.AddListener(OnKnifeCostIncreaseChanged);
                knifeCostIncreaseSlider.wholeNumbers = true;
            }

            UpdateUI();
        }

        if (customSettingsPanel != null)
        {
            customSettingsPanel.SetActive(false);
        }
    }

    private void OnDifficultyChanged(Difficulty newDifficulty)
    {
        if (customSettingsPanel != null)
        {
            customSettingsPanel.SetActive(newDifficulty == Difficulty.Custom);
        }

        if (newDifficulty == Difficulty.Custom)
        {
            customBalance = DataManager.Instance.GetCustomBalance();
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (customBalance == null) return;

        if (knifeBaseCostSlider != null)
        {
            knifeBaseCostSlider.minValue = 1;
            knifeBaseCostSlider.maxValue = 20;
            knifeBaseCostSlider.value = customBalance.knifeBaseCost;
            knifeBaseCostValue.text = $"base cost: {customBalance.knifeBaseCost}";
        }

        if (knifeCostIncreaseSlider != null)
        {
            knifeCostIncreaseSlider.minValue = 0;
            knifeCostIncreaseSlider.maxValue = 10;
            knifeCostIncreaseSlider.value = customBalance.knifeCostIncrease;
            knifeCostIncreaseValue.text = $"up cost: {customBalance.knifeCostIncrease}";
        }

        UpdatePreview();
    }

    private void OnKnifeBaseCostChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeBaseCost = intValue;
        knifeBaseCostValue.text = $"base cost: {intValue}";

        DataManager.Instance.SaveCustomBalance();

        UpdatePreview();
    }

    private void OnKnifeCostIncreaseChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeCostIncrease = intValue;
        knifeCostIncreaseValue.text = $"up cost: {intValue}";

        DataManager.Instance.SaveCustomBalance();

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (previewText != null && customBalance != null)
        {
            previewText.text = $"Текущие настройки Custom сложности:\n" +
                              $"Базовая стоимость ножа: {customBalance.knifeBaseCost}\n" +
                              $"Увеличение стоимости: {customBalance.knifeCostIncrease}\n";
        }
    }

    public void ResetToDefault()
    {
        if (customBalance == null) return;

        customBalance.knifeBaseCost = 5;
        customBalance.knifeCostIncrease = 3;

        UpdateUI();

        DataManager.Instance.SaveCustomBalance();

        Debug.Log("Custom balance reset to default");
    }

    private void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }
}