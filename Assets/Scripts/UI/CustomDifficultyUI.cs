using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

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

    [Header("Preview")]
    [SerializeField] private TMP_Text previewText;
    [SerializeField] private Slider difficultySlider;

    private GameBalance customBalance;

    private float russianSpacing = -42f;
    private float russianWordSpacing = 42f;
    private float defaultSpacing = 0f;
    private float defaultWordSpacing = 0f;

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
            SetParamsActive(false);
            UpdateUI();
        }

        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnShowCustomDifficulty += SetParamsActive;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnShowCustomDifficulty -= SetParamsActive;
    }

    private void SetParamsActive(bool is_active)
    {
        if (knifeBaseCostSlider != null)
        {
            knifeBaseCostSlider.interactable = is_active;
        }

        if (knifeCostIncreaseSlider != null)
        {
            knifeCostIncreaseSlider.interactable = is_active;
        }

        if (startNeedSlider != null)
        {
            startNeedSlider.interactable = is_active;
        }
        if (durationSlider != null)
        {
            durationSlider.interactable = is_active;
        }
        if (randomGrowthSlider != null)
        {
            randomGrowthSlider.interactable = is_active;
        }
        if (collectionNeedSlider != null)
        {
            collectionNeedSlider.interactable = is_active;
        }

        if (organSlider != null)
        {
            organSlider.interactable = is_active;
        }

        if (karmaRewardSlider != null)
        {
            karmaRewardSlider.interactable = is_active;
        }
    }

    private void OnDifficultyChanged(Difficulty newDifficulty)
    {

        if (newDifficulty == Difficulty.Easy)
        {
            customBalance = DataManager.Instance.GetEasyBalance();
        }
        if (newDifficulty == Difficulty.Normal)
        {
            customBalance = DataManager.Instance.GetNormalBalance();
        }
        if (newDifficulty == Difficulty.Hard)
        {
            customBalance = DataManager.Instance.GetHardBalance();
        }
        if (newDifficulty == Difficulty.Custom)
        {
            customBalance = DataManager.Instance.GetCustomBalance();
        }
        UpdateUI();
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
            karmaRewardSlider.value = customBalance.rewardForOne;
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
        customBalance.rewardForOne = intValue;
        customBalance.rewardForTwo = 3*intValue;
        customBalance.rewardForThree = 5*intValue;
        karmaRewardText.text = $"{intValue}";

        UpdatePreview();
    }

    private void UpdatePreview()
    {
        DataManager.Instance.SaveCustomBalance();
        if (previewText != null && customBalance != null)
        {
            difficultySlider.value = CalculateDifficulty();
            string difficultyKey = GetDifficultyKey();
            string localizedText = GetLocalizedString(difficultyKey);
            previewText.text = localizedText;
            AdjustCharacterSpacing();
        }
    }
    private string GetDifficultyKey()
    {
        float value = difficultySlider.value;

        if (value <= 2f)
            return "isEasy";
        else if (value <= 8f)
            return "isNorm";
        else
            return "isHard";
    }
    private string GetLocalizedString(string key)
    {
        var table = LocalizationSettings.StringDatabase.GetTable("GutsOutLocalizationTable");
        if (table != null)
        {
            var entry = table.GetEntry(key);
            if (entry != null)
            {
                return entry.GetLocalizedString();
            }
        }

        return key switch
        {
            "isEasy" => "Easy",
            "isNorm" => "Normal",
            "isHard" => "Hard",
            _ => "Unknown"
        };
    }
    private void AdjustCharacterSpacing()
    {
        if (previewText == null) return;

        string currentLanguage = LocalizationSettings.SelectedLocale.Identifier.Code;

        if (currentLanguage == "ru")
        {
            previewText.characterSpacing = russianSpacing;
            previewText.wordSpacing = russianWordSpacing;
        }
        else
        {
            previewText.characterSpacing = defaultSpacing;
            previewText.wordSpacing = defaultWordSpacing;
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

        float startFactor = (startNeed - 1f) / 4f;

        float knifeRisk = (knife) * (1f / (reward + 1f));

        float riskMultiplier = 1f;
        if (risk == -1) riskMultiplier = 1.5f;
        else if (risk == 1) riskMultiplier = 0.5f;

        float knifeFactor = (knifeRisk * riskMultiplier * 4f);

        float smooth = (1f + r) / (4f*r);

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
        UnsubscribeDependencies();
    }
}