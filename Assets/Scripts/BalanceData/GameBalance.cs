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

    [Header("Balance Prediction (Auto)")]
    [Tooltip("Среднее число дней до сбора 3 особых пациентов")]
    public float predictedDaysToWin = 0f;
    [Tooltip("Сложность от 1 (очень легко) до 10 (почти невозможно)")]
    public float predictedDifficulty = 0f;
    [Tooltip("Ожидаемый процент побед при нормальной игре (0..1)")]
    public float predictedWinRate = 0f;

#region MainMethods
    public int GetBaseReq()
    {
        return baseReqRelease;
//#if UNITY_EDITOR
//        return baseReqEditor;
//#else
//            return baseReqRelease;
//#endif

    }

    public int GetTypeReq()
    {
        return typeReqRelease;
//#if UNITY_EDITOR
//        return typeReqEditor;
//#else
//        return typeReqRelease;
//#endif
    }

    public (int reqStats, int downReqStats) CalculateRequirements(int currentDay)
    {
        float curr_day = (float)currentDay;

        float dayFactor = Mathf.Pow(curr_day, pow);
        int reqStats = (int)(startRec + mulDay * dayFactor);
        int downReqStats = (int)(reqStats / divLowReq);

        return (Mathf.Min(reqStats, 40), Mathf.Min(downReqStats, 25));
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
    #endregion

    public void SetOrganMul(int risk)
    {
        ordinaryMul = 0f;

        //(cursedMul, badMul, goodMul, rareMul, epicMul, legendaryMul) = risk switch
        //{
        //    -1 => (0.1f, 0.02f, 0f, 0.01f, 0.04f, 0.06f),
        //    1 => (0f, 0f, 0.02f, 0.04f, 0.08f, 0.1f),
        //    _ => (0.05f, 0.01f, 0.01f, 0.02f, 0.06f, 0.08f)
        //};
        (cursedMul, badMul, goodMul, rareMul, epicMul, legendaryMul) = risk switch
        {
            -1 => (0.04f, 0.01f, 0f, 0.01f, 0.03f, 0.04f),
            1 => (0.02f, 0.01f, 0.03f, 0.05f, 0.05f, 0.05f),
            _ => (0.03f, 0.02f, 0.02f, 0.03f, 0.04f, 0.05f)
        };
        (cursedBase, badBase, ordinaryBase, goodBase, rareBase, epicBase, legendaryBase) = risk switch
        {
            -1 => (0.4f, 0.8f, 0.8f, 0.3f, 0.1f, 0.01f, 0f),
            1 => (0.1f, 0.2f, 1f, 0.6f, 0.1f, 0.01f, 0f),
            _ => (0.2f, 0.5f, 1f, 0.5f, 0.1f, 0.02f, 0.01f)
        };
    }

}
