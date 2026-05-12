using System.Runtime.CompilerServices;
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

    // Collection Needs
#if UNITY_EDITOR
    private int base_req = 1;
    private int type_req = 2;
#else
    private int base_req = 10;
    private int type_req = 20;
#endif

    //Parameters of needs
    private const int startRec = 4;
    private const float mul_day = 0.08f;
    private const float div_low_req = 4f;

    private int reqStats;
    private int down_reqStats = 0;

    [Header("Container Type")]
    [SerializeField] private bool isCollectionContainer = false;
    
    public bool IsCollection => isCollectionContainer;

    [Header("Collection")]
    [SerializeField] private CategoryType category_type = CategoryType.None;
    public CategoryType CollectionCategory => category_type;

    public int GetRequiredMind() => ReqMind;
    public int GetRequiredSoul() => ReqSoul;
    public int GetRequiredBody() => ReqBody;

    private void Start()
    {
        reqStats = startRec;
        UpdateRequires();
        SetStats();
        CalculateStats();

        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDayEnd += UpdateRequires;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDayEnd -= UpdateRequires;
    }

    private void UpdateRequires()
    {
        if (!isCollectionContainer)
        {
            float curr_day = (float)GameManager.Instance.CurrentDay;
            reqStats = (int)((float)startRec + mul_day * (curr_day * Mathf.Sqrt(curr_day)));
            down_reqStats = (int)(reqStats / div_low_req);
        }
    }
    private void SetStats()
    {
        if (IsCollection)
        {
            ReqMind = base_req;
            ReqSoul = base_req;
            ReqBody = base_req;

            switch (category_type)
            {
                case CategoryType.Organ:
                    ReqSoul = type_req;
                    break;
                case CategoryType.Mechanic:
                    ReqMind = type_req;
                    break;
                case CategoryType.Insects:
                    ReqBody = type_req;
                    break;
                default:
                    break;

            }
        }
        else RandomRequireStats();
    }
    public void RandomRequireStats()
    {
        ReqMind = Random.Range(down_reqStats, reqStats);
        ReqSoul = Random.Range(down_reqStats, reqStats);
        ReqBody = Random.Range(down_reqStats, reqStats);
    }

    public void CalculateStats()
    {
        TotalMind = 0; TotalSoul = 0; TotalBody = 0;
        var organs = GetComponentsInChildren<OrganItem>(true);
        foreach (var organ in organs)
        {
            if (organ == null) continue;
            TotalMind += organ.GetStat(StatType.Mind);
            TotalSoul += organ.GetStat(StatType.Soul);
            TotalBody += organ.GetStat(StatType.Body);
        }
        if (isCollectionContainer && IsFulfilled)
        {
            EventBus.TriggerCollectionHumanReady();
        }
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}