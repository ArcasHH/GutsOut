using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Balance Presets")]
     private GameBalance easyBalance;
     private GameBalance normalBalance;
     private GameBalance hardBalance;
     private GameBalance customBalance;

    [Header("Current Settings")]
    [SerializeField] private Difficulty currentDifficulty = Difficulty.Normal;
    [SerializeField] private GameBalance currentBalance;

    [Header("Game Data")]
    public int totalKarma { get; private set; }
    public int knivesBought;
    public int currentDay { get; private set; } = 1;

    public int currentReqStats { get; private set; }
    public int currentDownReqStats { get; private set; } = 0;
    public System.Action<int, int> OnRequirementsChanged;
    public System.Action<Difficulty> OnDifficultyChanged;

    public Difficulty GetCurrentDifficulty() => currentDifficulty;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBalancesFromResources();
            LoadSettings();
            LoadCustomBalance();
            SetDifficulty(currentDifficulty);
            UpdateRequirements();
        }
        else
        {
            Destroy(gameObject);
        }
        SubscribeDependencies();
        SetInitParams();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDayEnd += EndDay;
        EventBus.OnGameStart += SetInitParams;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDayEnd -= EndDay;
        EventBus.OnGameStart -= SetInitParams;
    }

    private void SetInitParams()
    {
        totalKarma = 0;
        knivesBought = 0;
        currentDay = 1;
        currentReqStats = Balance.startRec;
        currentDownReqStats = 0;
    }
    private void EndDay()
    {
        currentDay++;
        UpdateRequirements();
    }
    public void AddScore(int score)
    {
        totalKarma += score;
    }

    private void LoadSettings()
    {
        currentDifficulty = (Difficulty)PlayerPrefs.GetInt("Difficulty", (int)Difficulty.Normal);
    }

    public void SetDifficulty(Difficulty newDifficulty)
    {
        currentDifficulty = newDifficulty;
        PlayerPrefs.SetInt("Difficulty", (int)newDifficulty);

        switch (newDifficulty)
        {
            case Difficulty.Easy:
                currentBalance = easyBalance;
                break;
            case Difficulty.Normal:
                currentBalance = normalBalance;
                break;
            case Difficulty.Hard:
                currentBalance = hardBalance;
                break;
            case Difficulty.Custom:
                currentBalance = customBalance;
                break;
        }

        OnDifficultyChanged?.Invoke(newDifficulty);
        UpdateRequirements();
    }

    public void UpdateRequirements()
    {
        if (currentBalance == null) return;

        var (req, downReq) = currentBalance.CalculateRequirements(currentDay);
        currentReqStats = req;
        currentDownReqStats = downReq;

        OnRequirementsChanged?.Invoke(currentReqStats, currentDownReqStats);

#if UNITY_EDITOR
        Debug.Log($"Day {currentDay}: Requirements updated - {currentReqStats} / {currentDownReqStats}");
#endif
    }

    private void LoadBalancesFromResources()
    {
        // Загружаем все ассеты типа GameBalance из папки BalanceData
        GameBalance[] allBalances = Resources.LoadAll<GameBalance>("BalanceData");

        foreach (var balance in allBalances)
        {
            // Ищем по имени ассета (без расширения)
            switch (balance.name)
            {
                case "GameBalance_EASY":
                    easyBalance = balance;
                    break;
                case "GameBalance_NORMAL":
                    normalBalance = balance;
                    break;
                case "GameBalance_HARD":
                    hardBalance = balance;
                    break;
                case "GameBalance_CUSTOM":
                    customBalance = balance;
                    break;
            }
        }

        // Проверяем, что все балансы загрузились
#if UNITY_EDITOR
    if (easyBalance == null) Debug.LogWarning("Easy balance not found in Resources/BalanceData");
    if (normalBalance == null) Debug.LogWarning("Normal balance not found in Resources/BalanceData");
    if (hardBalance == null) Debug.LogWarning("Hard balance not found in Resources/BalanceData");
    if (customBalance == null) Debug.LogWarning("Custom balance not found in Resources/BalanceData");
#endif
    }

    public GameBalance GetCustomBalance()
    {
        return customBalance;
    }

    public void SaveCustomBalance()
    {
        string json = JsonUtility.ToJson(customBalance);
        PlayerPrefs.SetString("CustomBalanceData", json);
        PlayerPrefs.Save();

        if (currentDifficulty == Difficulty.Custom)
        {
            currentBalance = customBalance;
        }

        //Debug.Log("Custom balance saved");
    }

    public void LoadCustomBalance()
    {
        if (PlayerPrefs.HasKey("CustomBalanceData"))
        {
            string json = PlayerPrefs.GetString("CustomBalanceData");
            JsonUtility.FromJsonOverwrite(json, customBalance);
            Debug.Log("Custom balance loaded");
        }
    }


    public float GetDropChanceBase(QualityType rarity) => currentBalance.GetDropChanceBase(rarity);
    public float GetDropChanceMul(QualityType rarity) => currentBalance.GetDropChanceMul(rarity);
    public int GetRewardForKarma(int count) => currentBalance.GetRewardByKarmaCount(count);
    public int GetKnifeBaseCost() => currentBalance.knifeBaseCost;
    public int GetKnifeCostIncrease() => currentBalance.knifeCostIncrease;

    public static GameBalance Balance => Instance.currentBalance;

    public static int RewardForOne => Instance.currentBalance.rewardForOne;
    public static int RewardForTwo => Instance.currentBalance.rewardForTwo;
    public static int RewardForThree => Instance.currentBalance.rewardForThree;
    public static float CursedBase => Instance.currentBalance.cursedBase;

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}

public enum Difficulty
{
    Easy,
    Normal,
    Hard,
    Custom
}

public static class Balance
{
    public static float CursedBase => DataManager.Balance.cursedBase;
    public static float BadBase => DataManager.Balance.badBase;
    public static float OrdinaryBase => DataManager.Balance.ordinaryBase;
    public static float GoodBase => DataManager.Balance.goodBase;
    public static float RareBase => DataManager.Balance.rareBase;
    public static float EpicBase => DataManager.Balance.epicBase;
    public static float LegendaryBase => DataManager.Balance.legendaryBase;

    public static int RewardForOne => DataManager.Balance.rewardForOne;
    public static int RewardForTwo => DataManager.Balance.rewardForTwo;
    public static int RewardForThree => DataManager.Balance.rewardForThree;

    public static int KnifeBaseCost => DataManager.Balance.knifeBaseCost;
    public static int KnifeCostIncrease => DataManager.Balance.knifeCostIncrease;

    public static int StartRec => DataManager.Balance.startRec;
    public static float MulDay => DataManager.Balance.mulDay;
    public static float Pow => DataManager.Balance.pow;
    public static float DivLowReq => DataManager.Balance.divLowReq;

    public static int GetBaseReq() => DataManager.Balance.GetBaseReq();
    public static int GetTypeReq() => DataManager.Balance.GetTypeReq();
    public static float GetDropChanceBase(QualityType rarity) => DataManager.Balance.GetDropChanceBase(rarity);
    public static float GetDropChanceMul(QualityType rarity) => DataManager.Balance.GetDropChanceMul(rarity);
    public static float GetWeightWithDayCoef(QualityType quality, float currentDay) => DataManager.Balance.GetWeightWithDayCoef(quality, currentDay);
    public static int GetRewardByKarmaCount(int count) => DataManager.Balance.GetRewardByKarmaCount(count);
    public static Dictionary<QualityType, float> GetAllWeightsWithDayCoef(float currentDay) => DataManager.Balance.GetAllWeightsWithDayCoef(currentDay);
}