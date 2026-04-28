using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrganObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ObjectType organType;
    
    [SerializeField] private GameObject stats_panel;
    [SerializeField] private TMP_Text statsText;

    private GameOrgan data;
    public GameOrgan Data => data;


    private void Start()
    {
        if (OrganRandomizer.Instance != null)
        {
            data = OrganRandomizer.Instance.GetRandomOrgan(organType);
        }

        if (data == null)
        {
            Debug.LogWarning($"[{name}] No data selected for type {organType}!");
            return;
        }

        SetColor();
        SetText();
        HideStatsPanel();
    }
    private void InitializeWithDelay()
    {
        if (OrganRandomizer.Instance != null)
        {
            data = OrganRandomizer.Instance.GetRandomOrgan(organType);
            if (data != null)
            {
                SetColor();
                SetText();
                HideStatsPanel();
                return;
            }
        }

        Debug.LogWarning($"Retrying to get organ for type {organType}...");
        Invoke(nameof(InitializeWithDelay), 0.1f);
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowStatsPanel();
    public void OnPointerExit(PointerEventData eventData) => HideStatsPanel();

    public int GetStat(StatType stat) => data?.GetStat(stat) ?? 0;

    private void ShowStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(true);
    }

    private void HideStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(false);
    }

    private void SetColor()
    {
        
        Image objImg = GetComponent<Image>();
        if (objImg == null || !ColorPaletteManager.Instance) return;
        Color objCol = Color.white;
        switch (data.qulity_type)
        {
            case QualityType.Bad:
                objCol = ColorPaletteManager.Instance.CurrentPalette.badOrganColor;
                break;

            case QualityType.Ordinary:
                objCol = ColorPaletteManager.Instance.CurrentPalette.ordinaryOrganColor;
                break;

            case QualityType.Good:
                objCol = ColorPaletteManager.Instance.CurrentPalette.goodOrganColor;
                break;

            default:
                objCol = Color.white;
                break;
        }

        objImg.color = objCol;
    }

    private void SetText()
    {
        if (statsText == null) return;
        statsText.text = $"{GetStat(StatType.Mind)}\n" +
                         $"{GetStat(StatType.Soul)}\n" +
                         $"{GetStat(StatType.Body)}";
    }
}