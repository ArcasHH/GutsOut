using UnityEngine;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Balance Presets")]
    [SerializeField] private GameBalance easyBalance;
    [SerializeField] private GameBalance normalBalance;
    [SerializeField] private GameBalance hardBalance;

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
            LoadSettings();
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
        }

        OnDifficultyChanged?.Invoke(newDifficulty);
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
    public float GetDropChance(QualityType rarity) => currentBalance.GetDropChance(rarity);
    public int GetRewardForKarma(int count) => currentBalance.GetRewardByKarmaCount(count);
    public int GetKnifeBaseCost() => currentBalance.humanDeleterBaseCost;
    public int GetKnifeCostIncrease() => currentBalance.humanDeleterCostIncrease;

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
    Hard
}

public static class Balance
{
    public static float DayMulCoef => DataManager.Balance.dayMulCoef;

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

    public static int KnifeBaseCost => DataManager.Balance.humanDeleterBaseCost;
    public static int KnifeCostIncrease => DataManager.Balance.humanDeleterCostIncrease;

    public static int StartRec => DataManager.Balance.startRec;
    public static float MulDay => DataManager.Balance.mulDay;
    public static float Pow => DataManager.Balance.pow;
    public static float DivLowReq => DataManager.Balance.divLowReq;

    public static int GetBaseReq() => DataManager.Balance.GetBaseReq();
    public static int GetTypeReq() => DataManager.Balance.GetTypeReq();
    public static float GetDropChance(QualityType rarity) => DataManager.Balance.GetDropChance(rarity);
    public static float GetWeightWithDayCoef(QualityType quality, float currentDay) => DataManager.Balance.GetWeightWithDayCoef(quality, currentDay);
    public static int GetRewardByKarmaCount(int count) => DataManager.Balance.GetRewardByKarmaCount(count);
    public static Dictionary<QualityType, float> GetAllWeightsWithDayCoef(float currentDay) => DataManager.Balance.GetAllWeightsWithDayCoef(currentDay);
}