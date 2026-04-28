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
    }

    private void OnEnable() => EventBus.OnInventoryChanged += ForceUpdate;
    private void OnDisable() => EventBus.OnInventoryChanged -= ForceUpdate;

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
        UpdateStat(MindStatsText, "mind", summarizer.TotalMind, summarizer.GetRequiredMind());
        UpdateStat(SoulStatsText, "soul", summarizer.TotalSoul, summarizer.GetRequiredSoul());
        UpdateStat(BodyStatsText, "body", summarizer.TotalBody, summarizer.GetRequiredBody());
    }

    private void UpdateStat(TMP_Text text, string name, int current, int required)
    {
        if (text == null) return;
        text.text = $"{name}: {current}/{required}";
        text.color = current < required ? wrongStatsColor : Color.white;
    }
}