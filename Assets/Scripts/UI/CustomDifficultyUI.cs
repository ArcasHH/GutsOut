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
    [SerializeField] private Slider startNeedSlider;
    [SerializeField] private TMP_Text startNeedText;

    [SerializeField] private Slider durationSlider;
    [SerializeField] private Slider randomGrowthSlider;

    [Header("Collection Needs")]
    [SerializeField] private Slider collectionNeedSlider;
    [SerializeField] private TMP_Text collectionNeedText;

    [Header("OrganProgression")]
    [SerializeField] private Slider organSlider;

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

            if (startNeedSlider != null)
            {
                startNeedSlider.onValueChanged.AddListener(OnStartNeedChanged);
            }
            if (durationSlider != null)
            {
                durationSlider.onValueChanged.AddListener(OnDurationChanged);
            }
            if (randomGrowthSlider != null)
            {
                randomGrowthSlider.onValueChanged.AddListener(OnrandomGrowthChanged);
            }
            if (collectionNeedSlider != null)
            {
                collectionNeedSlider.onValueChanged.AddListener(OnCollectionNeedChanged);
            }

            if (organSlider != null)
            {
                organSlider.onValueChanged.AddListener(OnOrganChanged);
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
        if (startNeedSlider != null)
        {
            startNeedSlider.value = customBalance.startRec;
        }

        if (durationSlider != null)
        {
            durationSlider.value = Mathf.Sqrt(customBalance.mulDay) * 10f;
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
        if (organSlider != null)
        {
            organSlider.value = 0;
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
        knifeBaseCostText.text = $"{intValue}";

        UpdatePreview();
    }

    private void OnKnifeCostIncreaseChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.knifeCostIncrease = intValue;
        knifeCostIncreaseText.text = $"{intValue}";

        UpdatePreview();
    }

    //Patients
    private void OnStartNeedChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.startRec = intValue;
        startNeedText.text = $"{intValue}";
        UpdatePreview();
    }
    private void OnDurationChanged(float value)
    {
        customBalance.mulDay = (float)(value * value * 0.01f);
        customBalance.pow = (float)(value * value * 0.025f - 0.5 * value + 3);
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
        collectionNeedText.text = $"{intValue}-{intValue * 2} ";
        UpdatePreview();
    }

    //Organs
    private void OnOrganChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.SetOrganMul(intValue);
        UpdatePreview();
    }

    //Karma
    private void OnKarmaRewardChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        customBalance.rewardForOne = 2*intValue;
        customBalance.rewardForOne = 6*intValue;
        customBalance.rewardForOne = 10*intValue;
        karmaRewardText.text = $"{intValue}";

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        DataManager.Instance.SaveCustomBalance();
        if (previewText != null && customBalance != null)
        {
            //previewText.text = customBalance.GetBalanceWarning();
            dayText.text = $"game length: {customBalance.predictedDaysToWin} days";
            
            difficultySlider.value = CalculateDifficulty();
            //if (customBalance.predictedDaysToWin == 0) difficultySlider.value = 10;
        }
        //customBalance.RecalculatePredictions();
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

    private float CalculateDifficulty()
    {
     
        float duration = durationSlider.value + 1f ;
        float startNeed = startNeedSlider.value;
        float r = randomGrowthSlider.value;
        float requires = collectionNeedSlider.value;
        float risk = organSlider.value;
        float knife = knifeCostIncreaseSlider.value;
        float reward = karmaRewardSlider.value;

        float timePressure = 0f;

        timePressure = (requires / duration);
        timePressure = Mathf.Clamp(timePressure, 0.1f, 100f);

        float startFactor = (startNeed - 1f) / 9f;

        float knifeRisk = (knife) * (1f / (reward + 1f));

        float riskMultiplier = 1f;
        if (risk == -1) riskMultiplier = 1.5f;
        else if (risk == 1) riskMultiplier = 0.5f;

        float knifeFactor = (knifeRisk * riskMultiplier * 2f);

        float smooth = (1f + r) / (2f * r);

        float rawDifficulty = timePressure * startFactor * knifeFactor;
        float adjusted = rawDifficulty * smooth;

        float finalDifficulty = Mathf.Clamp(adjusted * 2.5f, 0f, 100f);

        return finalDifficulty;
    }

    private void OnDestroy()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.OnDifficultyChanged -= OnDifficultyChanged;
        }
    }
}