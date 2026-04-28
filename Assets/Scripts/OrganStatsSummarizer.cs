using UnityEngine;

public class OrganStatsSummarizer : MonoBehaviour
{
    public int TotalMind { get; private set; }
    public int TotalSoul { get; private set; }
    public int TotalBody { get; private set; }

    private void Start()
    {
        CalculateStats();
    }

    public void CalculateStats()
    {
        TotalMind = 0;
        TotalSoul = 0;
        TotalBody = 0;

        var organs = GetComponentsInChildren<OrganObject>(true);

        foreach (var organ in organs)
        {
            if (organ == null) continue;

            TotalMind += organ.GetStat(StatType.Mind);
            TotalSoul += organ.GetStat(StatType.Soul);
            TotalBody += organ.GetStat(StatType.Body);
        }
    }

    [ContextMenu("🔄 Recalculate Stats")]
    private void EditorRecalculate() => CalculateStats();
}