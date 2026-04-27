using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatsDisplayUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private OrganStatsSummarizer summarizer;

    private void OnEnable()
    {
        SlotController.OnInventoryChanged += UpdateDisplay;
        UpdateDisplay();
    }

    private void OnDisable()
    {
        SlotController.OnInventoryChanged -= UpdateDisplay;
    }

    private void UpdateDisplay()
    {
        if (summarizer == null || statsText == null) return;

        summarizer.CalculateStats();

        statsText.text = $"Mind: {summarizer.TotalMind}\n" +
                         $"Soul: {summarizer.TotalSoul}\n" +
                         $"Instinct: {summarizer.TotalInstinct}";
    }
}