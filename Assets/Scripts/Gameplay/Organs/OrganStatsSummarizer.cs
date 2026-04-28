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

    public int GetRequiredMind() => ReqMind;
    public int GetRequiredSoul() => ReqSoul;
    public int GetRequiredBody() => ReqBody;

    // 🔑 Awake вызывается РАНЬШЕ Start() у всех объектов в сцене
    private void Awake()
    {
        RandomRequireStats();
        CalculateStats();
    }

    public void RandomRequireStats()
    {
        ReqMind = Random.Range(1, 6);
        ReqSoul = Random.Range(1, 6);
        ReqBody = Random.Range(1, 6);
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