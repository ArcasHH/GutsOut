using UnityEngine;
using TMPro;
using System.Collections;

public class StatsDisplayUI : MonoBehaviour
{
    [SerializeField] private TMP_Text MindStatsText;
    [SerializeField] private TMP_Text SoulStatsText;
    [SerializeField] private TMP_Text BodyStatsText;

    private OrganStatsSummarizer summarizer;
    private Color wrongStatsColor;

    private void Awake()
    {
        summarizer = GetComponentInParent<OrganStatsSummarizer>();
        wrongStatsColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnInventoryChanged += ForceUpdate;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnInventoryChanged -= ForceUpdate;
    }

    private void Start() => StartCoroutine(InitDisplayDelayed());

    private IEnumerator InitDisplayDelayed()
    {
        yield return null;
        ForceUpdate();
    }

    public void ForceUpdate()
    {
        if (summarizer == null) return;

        summarizer.CalculateStats();
        UpdateStat(MindStatsText, summarizer.TotalMind, summarizer.GetRequiredMind());
        UpdateStat(SoulStatsText, summarizer.TotalSoul, summarizer.GetRequiredSoul());
        UpdateStat(BodyStatsText, summarizer.TotalBody, summarizer.GetRequiredBody());
    }

    private void UpdateStat(TMP_Text text, int current, int required)
    {
        if (text == null) return;
        text.text = $" : {current}/{required}";
        text.color = current < required ? wrongStatsColor : Color.white;
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}