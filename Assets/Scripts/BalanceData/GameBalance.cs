using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameBalance", menuName = "Game/Game Balance")]
public class GameBalance : ScriptableObject
{
    [Header("Difficulty Settings")]
    public string difficultyName = "EASY";

    [Header("Drop Rates")]

    public float cursedBase = 0.2f;
    public float badBase = 0.5f;
    public float ordinaryBase = 1f;
    public float goodBase = 0.5f;
    public float rareBase = 0.2f;
    public float epicBase = 0.08f;
    public float legendaryBase = 0.01f;

    public float cursedMul = 0.02f;
    public float badMul = 0.01f;
    public float ordinaryMul = 0.005f;
    public float goodMul = 0.01f;
    public float rareMul = 0.02f;
    public float epicMul = 0.025f;
    public float legendaryMul = 0.03f;

    [Header("Karma Rewards (per day)")]
    public int rewardForOne = 10;
    public int rewardForTwo = 30;
    public int rewardForThree = 50;

    [Header("Knife")]
    public int knifeBaseCost = 5;
    public int knifeCostIncrease = 3;

    [Header("Player Needs")]
    public int startRec = 4;
    public float mulDay = 0.08f;
    public float pow = 1.5f;
    public float divLowReq = 4f;

    [Header("Production (Editor vs Release)")]
    public int baseReqEditor = 1;
    public int typeReqEditor = 2;
    public int baseReqRelease = 10;
    public int typeReqRelease = 20;

    public int GetBaseReq()
    {
#if UNITY_EDITOR
        return baseReqEditor;
#else
            return baseReqRelease;
#endif
    }

    public int GetTypeReq()
    {
#if UNITY_EDITOR
        return typeReqEditor;
#else
            return typeReqRelease;
#endif
    }

    public (int reqStats, int downReqStats) CalculateRequirements(int currentDay)
    {
        float curr_day = (float)currentDay;

        float dayFactor = Mathf.Pow(curr_day, pow);
        int reqStats = (int)(startRec + mulDay * dayFactor);
        int downReqStats = (int)(reqStats / divLowReq);

        return (reqStats, downReqStats);
    }

    public float GetDropChanceBase(QualityType rarity)
    {
        switch (rarity)
        {
            case QualityType.Cursed: return cursedBase;
            case QualityType.Bad: return badBase;
            case QualityType.Ordinary: return ordinaryBase;
            case QualityType.Good: return goodBase;
            case QualityType.Rare: return rareBase;
            case QualityType.Epic: return epicBase;
            case QualityType.Legendary: return legendaryBase;
            default: return 0;
        }
    }
    public float GetDropChanceMul(QualityType rarity)
    {
        switch (rarity)
        {
            case QualityType.Cursed: return cursedMul;
            case QualityType.Bad: return badMul;
            case QualityType.Ordinary: return ordinaryMul;
            case QualityType.Good: return goodMul;
            case QualityType.Rare: return rareMul;
            case QualityType.Epic: return epicMul;
            case QualityType.Legendary: return legendaryMul;
            default: return 0;
        }
    }
    public float GetWeightWithDayCoef(QualityType quality, float currentDay)
    {
        float baseWeight = GetDropChanceBase(quality);
        float mulWeight = GetDropChanceMul(quality);
        return baseWeight + mulWeight * currentDay;
    }

    public Dictionary<QualityType, float> GetAllWeightsWithDayCoef(float currentDay)
    {
        
        var weights = new Dictionary<QualityType, float>();

        foreach (QualityType quality in System.Enum.GetValues(typeof(QualityType)))
        {
            float coef = GetDropChanceMul(quality) * currentDay;
            weights[quality] = GetDropChanceBase(quality) + coef;
        }
        return weights;
    }

    public int GetRewardByKarmaCount(int count)
    {
        switch (count)
        {
            case 1: return rewardForOne;
            case 2: return rewardForTwo;
            case 3: return rewardForThree;
            default: return 0;
        }
    }
}
