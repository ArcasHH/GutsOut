using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static AudioButton;

public class OrganObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ObjectTypeData data;
    [SerializeField] private GameObject stats_panel;
    [SerializeField] private TMP_Text statsText;

    public ObjectTypeData Data => data;

    private void Awake()
    {
        if (data == null)
            Debug.LogWarning($"[{name}] No ObjectTypeData! ");

        SetColor();
        SetText();
        HideStatsPanel();
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
                objImg.color = ColorPaletteManager.Instance.CurrentPalette.badOrganColor;
                break;

            case QualityType.Ordinary:
                objImg.color = ColorPaletteManager.Instance.CurrentPalette.ordinaryOrganColor;
                break;

            case QualityType.Good:
                objImg.color = ColorPaletteManager.Instance.CurrentPalette.goodOrganColor;
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