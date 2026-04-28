using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsDisplayUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text MindStatsText;
    [SerializeField] private TMP_Text SoulStatsText;
    [SerializeField] private TMP_Text BodyStatsText;

    [SerializeField] private OrganStatsSummarizer summarizer;

    private Color wrongStatsColor;

    private void Start()
    {
        wrongStatsColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
        UpdateDisplay();
    }
    private void OnEnable()
    {
        EventBus.OnInventoryChanged += UpdateDisplay;
        UpdateDisplay();
    }

    private void OnDisable()
    {
        EventBus.OnInventoryChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (summarizer == null) return;

        summarizer.CalculateStats();

        UpdateStatDisplay(MindStatsText, "mind", summarizer.TotalMind, summarizer.GetRequiredMind());
        UpdateStatDisplay(SoulStatsText, "soul", summarizer.TotalSoul, summarizer.GetRequiredSoul());
        UpdateStatDisplay(BodyStatsText, "body", summarizer.TotalBody, summarizer.GetRequiredBody());
    }

    private void UpdateStatDisplay(TMP_Text textComponent, string statName, int current, int required)
    {
        textComponent.text = $"{statName}: {current}/{required}";
        textComponent.color = current < required ? wrongStatsColor : Color.white;
    }
}