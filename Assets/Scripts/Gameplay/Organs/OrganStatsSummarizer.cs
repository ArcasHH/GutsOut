using System.Runtime.CompilerServices;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
        SetStats();
        CalculateStats();

    }
    private void SetStats()
    {
        if (IsCollection)
        {
            int base_req = Balance.GetBaseReq();
            int type_req = Balance.GetTypeReq();
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
        int down_reqStats = DataManager.Instance.currentReqStats;
        int reqStats = DataManager.Instance.currentDownReqStats;
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
}