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

    [Header("OrganProgression")]
    [SerializeField] private Slider organInitSlider;
    [SerializeField] private Slider organGrowthSlider;
    [SerializeField] private Slider organRiskSlider;

    [Header("Karma Reward")]
    [SerializeField] private Slider karmaRewardSlider;
    [SerializeField] private TMP_Text karmaRewardText;

    [Header("UI References")]
    [SerializeField] private GameObject customSettingsPanel;

    [Header("Preview")]
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private Slider difficultySlider;

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

            if (organInitSlider != null)
            {
                organInitSlider.onValueChanged.AddListener(OnOrganInitChanged);
            }
            if (organGrowthSlider != null)
            {
                organGrowthSlider.onValueChanged.AddListener(OnOrganGrowthChanged);
            }
            if (organRiskSlider != null)
            {
                organRiskSlider.onValueChanged.AddListener(OnOrganRiskChanged);
            }

            if (karmaRewardSlider != null)
            {
                karmaRewardSlider.onValueChanged.AddListener(OnKarmaRewardChanged);
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

        //Knife
        if (knifeBaseCostSlider != null)
        {
            knifeBaseCostSlider.value = customBalance.knifeBaseCost;
        }

        if (knifeCostIncreaseSlider != null)
        {
            knifeCostIncreaseSlider.value = customBalance.knifeCostIncrease;
        }

        //Needs
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
            collectionNeedSlider.value = customBalance.baseReqRelease ;
        }

        //Organ settings
        if (organInitSlider != null)
        {
            organInitSlider.value = customBalance.initialWealth*5f;
        }

        if (organGrowthSlider != null)
        {
            organGrowthSlider.value = customBalance.progressionSpeed;
        }

        if (organRiskSlider != null)
        {
            organRiskSlider.value = customBalance.riskRewardBalance;
        }

        if (karmaRewardSlider != null)
        {
            karmaRewardSlider.value = customBalance.rewardForOne * 2;
        }

        UpdatePreview();
    }
    //KNife
    private void OnKnifeBaseCostChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeBaseCost = intValue;
        knifeBaseCostText.text = $"base cost: {intValue}";

        UpdatePreview();
    }

    private void OnKnifeCostIncreaseChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeCostIncrease = intValue;
        knifeCostIncreaseText.text = $"up cost: {intValue}";

        UpdatePreview();
    }

    //Patients
    private void OnNeedGrowthChanged(float value)
    {
        customBalance.mulDay = value;
        UpdatePreview();
    }
    private void OnLinearGrowthChanged(float value)
    {
        customBalance.pow = value;
        UpdatePreview();
    }
    private void OnrandomGrowthChanged(float value)
    {
        customBalance.divLowReq = value;
        UpdatePreview();
    }
    //Collection
    private void OnCollectionNeedChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.baseReqRelease = intValue;
        customBalance.typeReqRelease = intValue * 2;
        UpdatePreview();
    }

    //Organs
    private void OnOrganInitChanged(float value)
    {
        customBalance.initialWealth = value/5f;
        UpdatePreview();
    }
    private void OnOrganGrowthChanged(float value)
    {
        customBalance.progressionSpeed = value;
        UpdatePreview();
    }
    private void OnOrganRiskChanged(float value)
    {
        customBalance.riskRewardBalance = value;
        UpdatePreview();
    }
    //Karma
    private void OnKarmaRewardChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.rewardForOne = 2*intValue;
        customBalance.rewardForOne = 6*intValue;
        customBalance.rewardForOne = 10*intValue;
        karmaRewardText.text = $"min reward: {intValue}";

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        DataManager.Instance.SaveCustomBalance();
        if (previewText != null && customBalance != null)
        {
            previewText.text = customBalance.GetBalanceWarning();
            dayText.text = $"game length: {customBalance.predictedDaysToWin} days";
            
            difficultySlider.value = customBalance.predictedDifficulty;
            if (customBalance.predictedDaysToWin == 0) difficultySlider.value = 10;
        }
        customBalance.RecalculatePredictions();
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