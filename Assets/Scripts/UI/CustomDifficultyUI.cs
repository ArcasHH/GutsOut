using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomDifficultyUI : MonoBehaviour
{
    [Header("Knife Settings")]
    [SerializeField] private Slider knifeBaseCostSlider;
    [SerializeField] private TMP_Text knifeBaseCostText;

    [SerializeField] private Slider knifeCostIncreaseSlider;
    [SerializeField] private TMP_Text knifeCostIncreaseText;

    [Header("Patient Needs")]
    [SerializeField] private Slider needGrowthSlider;
    [SerializeField] private Slider linearGrowthSlider;
    [SerializeField] private Slider randomGrowthSlider;

    [Header("Collection Needs")]
    [SerializeField] private Slider collectionNeedSlider;
    [SerializeField] private TMP_Text collectionNeedText;

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

            if (needGrowthSlider != null)
            {
                needGrowthSlider.onValueChanged.AddListener(OnNeedGrowthChanged);
            }
            if (linearGrowthSlider != null)
            {
                linearGrowthSlider.onValueChanged.AddListener(OnLinearGrowthChanged);
            }
            if (randomGrowthSlider != null)
            {
                randomGrowthSlider.onValueChanged.AddListener(OnrandomGrowthChanged);
            }
            if (collectionNeedSlider != null)
            {
                collectionNeedSlider.onValueChanged.AddListener(OnCollectionNeedChanged);
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
            knifeBaseCostSlider.value = customBalance.knifeBaseCost;
        }

        if (knifeCostIncreaseSlider != null)
        {
            knifeCostIncreaseSlider.value = customBalance.knifeCostIncrease;
        }


        if (needGrowthSlider != null)
        {
            needGrowthSlider.value = customBalance.mulDay;
        }

        if (linearGrowthSlider != null)
        {
            linearGrowthSlider.value = customBalance.pow;
        }

        if (randomGrowthSlider != null)
        {
            randomGrowthSlider.value = customBalance.divLowReq;
        }

        if (collectionNeedSlider != null)
        {
            collectionNeedSlider.value = ((float)customBalance.typeReqRelease / 40f) ;
        }

        UpdatePreview();
    }

    private void OnKnifeBaseCostChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeBaseCost = intValue;
        knifeBaseCostText.text = $"base cost: {intValue}";

        DataManager.Instance.SaveCustomBalance();

        UpdatePreview();
    }

    private void OnKnifeCostIncreaseChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeCostIncrease = intValue;
        knifeCostIncreaseText.text = $"up cost: {intValue}";

        DataManager.Instance.SaveCustomBalance();

        UpdatePreview();
    }

    //Patients
    private void OnNeedGrowthChanged(float value)
    {
        customBalance.mulDay = value;
        DataManager.Instance.SaveCustomBalance();
        UpdatePreview();
    }
    private void OnLinearGrowthChanged(float value)
    {
        customBalance.pow = value;
        DataManager.Instance.SaveCustomBalance();
        UpdatePreview();
    }
    private void OnrandomGrowthChanged(float value)
    {
        customBalance.divLowReq = value;
        DataManager.Instance.SaveCustomBalance();
        UpdatePreview();
    }
    //Collection
    private void OnCollectionNeedChanged(float value)
    {
        customBalance.baseReqRelease = (int)(value * 21f);
        customBalance.typeReqRelease = (int)(value * 45f);
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