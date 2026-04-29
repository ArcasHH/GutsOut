using UnityEngine;

public class OrganStatsSummarizer : MonoBehaviour
{
    public int TotalMind { get; private set; }
    public int TotalSoul { get; private set; }
    public int TotalBody { get; private set; }

    public int ReqMind { get; private set; }
    public int ReqSoul { get; private set; }
    public int ReqBody { get; private set; }

    public bool IsFulfilled => TotalMind >= ReqMind && TotalSoul >= ReqSoul && TotalBody >= ReqBody;

    [Header("Тип контейнера")]
    [Tooltip("Если true, контейнер не участвует в смене дня и никогда не исчезает")]
    [SerializeField] private bool isCollectionContainer = false;
    
    public bool IsCollection => isCollectionContainer;

    [Header("Коллекция")]
    [Tooltip("Если isCollectionContainer == false, то принимает любые предметы")]
    [SerializeField] private CategoryType category_type = CategoryType.None;
    public CategoryType CollectionCategory => category_type;

    public int GetRequiredMind() => ReqMind;
    public int GetRequiredSoul() => ReqSoul;
    public int GetRequiredBody() => ReqBody;

    private void Awake()
    {
        RandomRequireStats();
        CalculateStats();
    }

    public void RandomRequireStats()
    {
        if (IsCollection) {
            ReqMind = 15;
            ReqSoul = 15;
            ReqBody = 15;
        }
        else {
            ReqMind = Random.Range(1, 6);
            ReqSoul = Random.Range(1, 6);
            ReqBody = Random.Range(1, 6);
        }
    }

    public void CalculateStats()
    {
        TotalMind = 0; TotalSoul = 0; TotalBody = 0;
        var organs = GetComponentsInChildren<OrganObject>(true);
        foreach (var organ in organs)
        {
            if (organ == null) continue;
            TotalMind += organ.GetStat(StatType.Mind);
            TotalSoul += organ.GetStat(StatType.Soul);
            TotalBody += organ.GetStat(StatType.Body);
        }
    }
}