using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class QualityWeight
{
    public QualityType quality;
    [Range(0, 100)] public float weight = 1f;
}

public class OrganRandomizer : MonoBehaviour
{
    [Header("QualityProbabilities")]
    [SerializeField]
    private List<QualityWeight> qualityWeights = new List<QualityWeight>();
    

    [Header("FolderPaths")]
    [SerializeField] private string baseFolderPath = "ScriptableOrgans";

    private Dictionary<ObjectType, List<GameOrgan>> organsByType;
    private Dictionary<QualityType, float> qualityProbability;
    private float totalQualityWeight;

    private static OrganRandomizer instance;
    public static OrganRandomizer Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("OrganRandomizer");
                instance = obj.AddComponent<OrganRandomizer>();
                DontDestroyOnLoad(obj);

                instance.Initialize();
            }
            return instance;
        }
    }
    private bool isInitialized = false;


    private const float day_mul_coef = 0.01f;

    private const float cursed_base = 0.2f;
    private const float bad_base = 0.5f;
    private const float ordinary_base = 1f;
    private const float good_base = 0.5f;
    private const float rare_base = 0.2f;
    private const float epic_base = 0.08f;
    private const float legendary_base = 0.01f;

    private void Initialize()
    {
        if (isInitialized) return;

        LoadAllOrgans();
        SetQualityWeights();
        CalculateQualityProbabilities();
        isInitialized = true;

        //Debug.Log("OrganRandomizer initialized manually");

        SubscribeDependencies();
        
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        LoadAllOrgans();
        CalculateQualityProbabilities();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDayEnd += SetQualityWeights;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDayEnd -= SetQualityWeights;
    }
    private void LoadAllOrgans()
    {
        organsByType = new Dictionary<ObjectType, List<GameOrgan>>();

        foreach (ObjectType type in System.Enum.GetValues(typeof(ObjectType)))
        {
            string folderPath = $"{baseFolderPath}/{type}";
            GameOrgan[] organs = Resources.LoadAll<GameOrgan>(folderPath);
            organsByType[type] = new List<GameOrgan>(organs);

        }
    }

    private void CalculateQualityProbabilities()
    {
        qualityProbability = new Dictionary<QualityType, float>();
        totalQualityWeight = 0;

        foreach (var qw in qualityWeights)
        {
            qualityProbability[qw.quality] = qw.weight;
            totalQualityWeight += qw.weight;
        }
    }

    public GameOrgan GetRandomOrgan(ObjectType type)
    {
        if (!organsByType.ContainsKey(type) || organsByType[type].Count == 0)
        {
            Debug.LogError($"No organs found for type {type}");
            return null;
        }

        QualityType selectedQuality = GetRandomQuality();

        var filteredOrgans = organsByType[type].Where(o => o.quality_type == selectedQuality).ToList();

        if (filteredOrgans.Count == 0)
        {
            Debug.LogWarning($"No {selectedQuality} organs found for type {type}, taking any quality");
            filteredOrgans = organsByType[type];
        }

        return filteredOrgans[Random.Range(0, filteredOrgans.Count)];
    }

    private QualityType GetRandomQuality()
    {
        float randomValue = Random.Range(0, totalQualityWeight);
        float currentWeight = 0;

        foreach (var qw in qualityWeights)
        {
            currentWeight += qw.weight;
            if (randomValue <= currentWeight)
                return qw.quality;
        }

        return QualityType.Ordinary; // fallback
    }

    public GameOrgan GetRandomOrganWithExactQuality(ObjectType type, QualityType quality)
    {
        if (!organsByType.ContainsKey(type)) return null;

        var filtered = organsByType[type].Where(o => o.quality_type == quality).ToList();
        return filtered.Count > 0 ? filtered[Random.Range(0, filtered.Count)] : null;
    }

    private void SetQualityWeights()
    {
        float curr_day = GameManager.Instance.CurrentDay;
        float coef = day_mul_coef * curr_day;
        qualityWeights = new List<QualityWeight>()
        {
            new QualityWeight() { quality = QualityType.Cursed, weight = cursed_base + coef},
            new QualityWeight() { quality = QualityType.Bad, weight = bad_base  + coef },
            new QualityWeight() { quality = QualityType.Ordinary, weight = ordinary_base  + coef},
            new QualityWeight() { quality = QualityType.Good, weight = good_base  + coef },
            new QualityWeight() { quality = QualityType.Rare, weight = rare_base  + coef },
            new QualityWeight() { quality = QualityType.Epic, weight = epic_base  + coef },
            new QualityWeight() { quality = QualityType.Legendary, weight = legendary_base  + coef }
        };
        CalculateQualityProbabilities();
    }


    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}