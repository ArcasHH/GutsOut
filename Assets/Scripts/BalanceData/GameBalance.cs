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
    #endregion

    // --------------------------------------------------------------
    // The average price of organs on a particular day
    // --------------------------------------------------------------
    private float GetAverageOrganPower(int day)
    {
        float totalWeight = 0f;
        float weightedSum = 0f;

        foreach (QualityType q in System.Enum.GetValues(typeof(QualityType)))
        {
            float weight = GetWeightWithDayCoef(q, day);
            totalWeight += weight;
            weightedSum += weight * GetOrganSumByRarity(q);
        }

        if (totalWeight <= 0f) return 0f;
        return weightedSum / totalWeight;
    }

    // Total parameters of the organ (mind+soul+body) by rarity .. For simplicity, you can extend it via ScriptableObject or dictionary.
    private float GetOrganSumByRarity(QualityType q)
    {
        switch (q)
        {
            case QualityType.Cursed: return 8f;
            case QualityType.Bad: return 2f;
            case QualityType.Ordinary: return 2f;
            case QualityType.Good: return 4f;
            case QualityType.Rare: return 8f;
            case QualityType.Epic: return 14f;
            case QualityType.Legendary: return 24f;
            default: return 2f;
        }
    }

    // --------------------------------------------------------------
    // Average requirement of one patient per day d (summary  mind+soul+body)
    // --------------------------------------------------------------
    private float GetAveragePatientRequirementSum(int day)
    {
        var (maxReq, minReq) = CalculateRequirements(day);
        return  3f * (minReq + maxReq) / 2f;
    }



    // --------------------------------------------------------------
    // Collection - Requirements of three special patients (total)
    // --------------------------------------------------------------
    private float GetSpecialPatientsTotalRequirement()
    {
        int baseReq = GetBaseReq();
        int typeReq = GetTypeReq();
        float perSpecial = baseReq + baseReq + typeReq;
        return perSpecial * 3f;
    }

    // --------------------------------------------------------------
    // Difficulty from 1 to 10
    // --------------------------------------------------------------

    public float CalculateDifficulty()
    {
        // 1. Оцениваем, насколько сложно собрать 3х пациентов в текущий день (средний день)
        // Берём день 10 как репрезентативный (середина типичной игры)
        int typicalDay = Mathf.Min(100, Mathf.Max(1, Mathf.RoundToInt(predictedDaysToWin / 2f)));

        // Среднее предложение органов в день (сила)
        float avgOrganPower = GetAverageOrganPower(typicalDay);
        float organsPerDayTotal = 2f * 4f * 0.75f; // 6 органов в обороте в день
        float totalDailySupply = organsPerDayTotal * avgOrganPower;

        // Средний спрос одного пациента (сумма трёх параметров)
        float demandPerPatient = GetAveragePatientRequirementSum(typicalDay);

        // Сколько пациентов можно покрыть в день имеющимися органами (теоретически)
        float maxPatientsPerDayRaw = totalDailySupply / demandPerPatient;

        // 2. Учитываем перераспределение (неидеальное покрытие mind/soul/body)
        // Чем больше разброс требований, тем сложнее собрать 3х
        var (maxReq, minReq) = CalculateRequirements(typicalDay);
        float requirementSpread = (maxReq - minReq) / (float)maxReq; // 0..1, чем выше, тем сложнее

        // Практическое кол-во пациентов, которых можно собрать
        float practicalPatientsPerDay = maxPatientsPerDayRaw * (1f - requirementSpread * 0.3f);


        int knifeCost = knifeBaseCost;
        int avgKarmaPerDay = 0;

        if (practicalPatientsPerDay >= 3f)
            avgKarmaPerDay = rewardForThree; // 50
        else if (practicalPatientsPerDay >= 2f)
            avgKarmaPerDay = rewardForTwo;   // 30
        else if (practicalPatientsPerDay >= 1f)
            avgKarmaPerDay = rewardForOne;   // 10
        else
            avgKarmaPerDay = 0;

        float knifeAffordability = (float)avgKarmaPerDay / knifeCost; // >1 = easy, <1 = hard

        // 4. Возможность откладывать органы (излишек сверх потребности 3х пациентов)
        float surplusForStorage = Mathf.Max(0f, practicalPatientsPerDay - 3f) / 3f; // 0..1, сколько сверх 3х пациентов

        // 5. Итоговая формула сложности (0-10)
        // Базовый фактор: сколько пациентов в день удаётся собирать
        float patientDifficulty = Mathf.Clamp01((3f - practicalPatientsPerDay) / 2.5f);
        // Если practicalPatientsPerDay >= 3 → 0; если =0.5 → 1; если =1 → 0.8

        // Фактор ножа (если кармы на нож не хватает, сложность растёт)
        float knifeDifficulty = Mathf.Clamp01(1f / knifeAffordability) * 0.3f;

        // Фактор откладывания (если нет излишков, сложнее копить на будущее)
        float storageDifficulty = Mathf.Clamp01(1f - surplusForStorage) * 0.2f;

        // Суммируем и нормализуем к 0-10
        float rawDifficulty = (patientDifficulty + knifeDifficulty + storageDifficulty) / 1.5f * 10f;

        predictedDifficulty = Mathf.Clamp(rawDifficulty, 0f, 10f);

        // Доп. логика: если практических пациентов меньше 1, то сложность всегда 9-10
        if (practicalPatientsPerDay < 0.8f)
            predictedDifficulty = Mathf.Max(predictedDifficulty, 9f);
        else if (practicalPatientsPerDay < 1.2f)
            predictedDifficulty = Mathf.Max(predictedDifficulty, 7f);

        return predictedDifficulty;
    }


    // --------------------------------------------------------------
    // 7. Проходимость (вероятность победы при нормальной игре)
    // --------------------------------------------------------------
    public float CalculateWinRate()
    {
        float daysToWin = CalculateDaysToWin();

        // Оцениваем средний баланс supply/demand за период игры
        float avgBalance = 0f;
        for (int d = 1; d <= daysToWin; d++)
        {
            float supply = 2f * 4f * GetAverageOrganPower(d);
            float demand = 3f * GetAveragePatientRequirementSum(d);
            avgBalance += supply / demand;
        }
        avgBalance /= daysToWin;

        // Логистическая функция
        // avgBalance = 1.2 -> 90% побед, avgBalance = 0.8 -> 20% побед
        float winRate = 1f / (1f + Mathf.Exp(-10f * (avgBalance - 1f)));
        winRate = Mathf.Clamp01(winRate);

        predictedWinRate = winRate;
        return winRate;
    }
    // Проверка, возможна ли победа в принципе (быстрая оценка)
    public bool IsWinPossible()
    {
        // Смотрим на 100-й день (бесконечность)
        float avgPowerLongTerm = GetAverageOrganPower(100);
        float demandLongTerm = GetAveragePatientRequirementSum(100);
        float netDailyLongTerm = 2f * 4f * avgPowerLongTerm - 1.5f * demandLongTerm;

        return netDailyLongTerm > 0f;
    }
    // --------------------------------------------------------------
    // 5. Среднее число дней до победы (решение уравнения)
    // --------------------------------------------------------------
    //public float CalculateDaysToWin()
    //{
    //    float requiredPower = GetSpecialPatientsTotalRequirement() / 0.49f; // efficiency 0.7 * utilRate 0.7
    //    float cumulative = 0f;

    //    for (int days = 1; days <= 500; days++) // защита от вечного цикла
    //    {
    //        cumulative += 2f * 4f * 0.75f * GetAverageOrganPower(days);
    //        if (cumulative >= requiredPower)
    //        {
    //            predictedDaysToWin = days;
    //            return days;
    //        }
    //    }
    //    predictedDaysToWin = 500f;
    //    return 500f;
    //}
    public float CalculateDaysToWin()
    {
        float specialRequirement = GetSpecialPatientsTotalRequirement();
        float accumulatedPower = 0f;
        float previousAccumulated = 0f;
        int daysWithoutProgress = 0;

        for (int day = 1; day <= 100; day++) // защита от бесконечности
        {
            // 1. Сколько органов поступает в оборот в этот день
            float newOrgansPower = 2f * 4f * GetAverageOrganPower(day); // 8 органов в день * среднюю силу

            // 2. Сколько нужно потратить на обычных пациентов, чтобы выжить
            float demandPerPatient = GetAveragePatientRequirementSum(day);

            // Сколько пациентов нужно вылечить, чтобы не проиграть?
            // Минимум - 1 пациент в день (иначе игра застопорится)
            // В среднем игрок лечит 1.5 пациентов в день для стабильного прогресса
            float patientsToHeal = Mathf.Clamp(1.5f, 1f, 3f);
            float costToSurvive = patientsToHeal * demandPerPatient;

            // 3. Чистый доход в коллекцию (что остаётся после лечения обычных)
            float netGain = newOrgansPower - costToSurvive;

            // Если чистый доход отрицательный, значит игрок тратит накопленное
            if (netGain < 0)
            {
                // Проверяем, можем ли мы вообще когда-либо накопить
                // Если отрицательный доход в течение 10 дней подряд и нет запаса → невозможно
                if (accumulatedPower <= 0 && day > 10)
                {
                    float futureAvgPower = GetAverageOrganPower(day + 10);
                    float futureDemand = GetAveragePatientRequirementSum(day + 10);
                    float futureNetGain = 2f * 4f * futureAvgPower - 1.5f * futureDemand;

                    if (futureNetGain < 0)
                    {
                        predictedDaysToWin = 0f;
                        return 0f; // Невозможно накопить никогда
                    }
                }

                // Тратим накопленное, но не уходим в минус
                accumulatedPower = Mathf.Max(0f, accumulatedPower + netGain);
            }
            else
            {
                // Накопляем
                accumulatedPower += netGain;
            }

            // 4. Проверяем, хватило ли накопленного на особых пациентов
            if (accumulatedPower >= specialRequirement)
            {
                predictedDaysToWin = day;
                return day;
            }

            // 5. Отслеживаем стагнацию (если 20 дней нет прогресса)
            if (Mathf.Abs(accumulatedPower - previousAccumulated) < specialRequirement * 0.01f)
            {
                daysWithoutProgress++;
                if (daysWithoutProgress >= 20)
                {
                    predictedDaysToWin = 0f;
                    return 0f; // Застряли, не можем накопить
                }
            }
            else
            {
                daysWithoutProgress = 0;
            }

            previousAccumulated = accumulatedPower;
        }

        // Не накопили за 1000 дней
        predictedDaysToWin = 0f;
        return 0f;
    }

    public void SetOrganMul(int risk)
    {
        ordinaryMul = 0f;

        (cursedMul, badMul, goodMul, rareMul, epicMul, legendaryMul) = risk switch
        {
            -1 => (0.1f, 0.02f, 0f, 0.02f, 0.06f, 0.08f),
            1 => (0f, 0f, 0.01f, 0.04f, 0.08f, 0.1f),
            _ => (0.05f, 0.01f, 0.05f, 0.03f, 0.07f, 0.09f)
        };
    }

    private void NormalizeDropRates()
    {
        // Нормализуем Base, чтобы сумма была между 1 и 5
        float totalBase = cursedBase + badBase + ordinaryBase + goodBase + rareBase + epicBase + legendaryBase;
        if (totalBase > 5f)
        {
            float scale = 5f / totalBase;
            cursedBase *= scale;
            badBase *= scale;
            ordinaryBase *= scale;
            goodBase *= scale;
            rareBase *= scale;
            epicBase *= scale;
            legendaryBase *= scale;
        }
        else if (totalBase < 1f && totalBase > 0f)
        {
            float scale = 1f / totalBase;
            cursedBase *= scale;
            badBase *= scale;
            ordinaryBase *= scale;
            goodBase *= scale;
            rareBase *= scale;
            epicBase *= scale;
            legendaryBase *= scale;
        }

        // Mul не нормализуем (они могут быть любыми)
    }


    // Получить максимальный день, до которого игра проходима
    public int GetMaxSurvivableDay()
    {
        for (int day = 1; day <= 500; day++)
        {
            float supply = 2f * 4f * GetAverageOrganPower(day);
            float demand = 1.5f * GetAveragePatientRequirementSum(day); // минимум для выживания

            if (supply < demand)
                return day - 1;
        }
        return 500;
    }

    public string GetBalanceWarning()
    {
        float days = CalculateDaysToWin();

        if (days <= 0f)
            return "НЕПРОХОДИМО! Слишком высокие требования пациентов";
        else if (days < 10f)
            return "Очень легко, пройдёте быстро";
        else if (days < 20f)
            return "Нормальная сложность";
        else if (days < 40f)
            return "Сложно, потребуется стратегия";
        else
            return "Экстрим, только для опытных";
    }

    // --------------------------------------------------------------
    // Обновить все (вызывать в Editor или при старте)
    // --------------------------------------------------------------
    [ContextMenu("Recalculate Balance Predictions")]
    public void RecalculatePredictions()
    {
        CalculateDaysToWin();
        CalculateDifficulty();
        CalculateWinRate();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RecalculatePredictions();
    }
#endif


}
