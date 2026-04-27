using UnityEngine;

public class OrganStatsSummarizer : MonoBehaviour
{
    public int TotalMind { get; private set; }
    public int TotalSoul { get; private set; }
    public int TotalInstinct { get; private set; }

    private void Start()
    {
        CalculateStats();
    }

    public void CalculateStats()
    {
        TotalMind = 0;
        TotalSoul = 0;
        TotalInstinct = 0;

        var organs = GetComponentsInChildren<OrganObject>(true);

        foreach (var organ in organs)
        {
            if (organ == null) continue;

            TotalMind += organ.GetStat(StatType.Mind);
            TotalSoul += organ.GetStat(StatType.Soul);
            TotalInstinct += organ.GetStat(StatType.Instinct);
        }
    }

    [ContextMenu("🔄 Recalculate Stats")]
    private void EditorRecalculate() => CalculateStats();
}