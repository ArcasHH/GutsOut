using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

public class OrganObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ObjectType organType;
    
    [SerializeField] private GameObject stats_panel;
    [SerializeField] private TMP_Text statsText;

    private GameOrgan data;
    private Image organImage;
    private Outline outline;
    public GameOrgan Data => data;
    private string spritesPath = "Sprites/GameSprites/Organs";

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
        organImage = GetComponent<Image>();
        outline = organImage.gameObject.GetComponent<Outline>();

        SetColor();
        SetText();
        HideStatsPanel();

        SetImage();
        SetOutline();
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowStatsPanel();
    public void OnPointerExit(PointerEventData eventData) => HideStatsPanel();

    public int GetStat(StatType stat) => data?.GetStat(stat) ?? 0;

    private void ShowStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(true);
        outline.enabled = true;
    }

    private void HideStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(false);
        outline.enabled = false;
    }
    private void SetImage()
    {
        if (organImage == null) return;

        Sprite[] allSprites = Resources.LoadAll<Sprite>(spritesPath);

        if (allSprites == null || allSprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found at path: {spritesPath}");
            return;
        }

        string targetSpriteName = $"{data.category_type}_{organType}";

        Sprite targetSprite = System.Array.Find(allSprites, s => s.name == targetSpriteName);

        if (targetSprite != null)
        {
            organImage.sprite = targetSprite;
        }
        else
        {
            Debug.LogWarning($"Sprite not found: {targetSpriteName} in {spritesPath}");
        }
    }
    private void SetColor()
    {
        if (organImage == null || !ColorPaletteManager.Instance) return;
        Color objCol = Color.white;
        switch (data.quality_type)
        {
            case QualityType.Cursed:
                objCol = ColorPaletteManager.Instance.CurrentPalette.cursedOrganColor;
                break;
            case QualityType.Bad:
                objCol = ColorPaletteManager.Instance.CurrentPalette.badOrganColor;
                break;

            case QualityType.Ordinary:
                objCol = ColorPaletteManager.Instance.CurrentPalette.ordinaryOrganColor;
                break;

            case QualityType.Good:
                objCol = ColorPaletteManager.Instance.CurrentPalette.goodOrganColor;
                break;
            case QualityType.Rare:
                objCol = ColorPaletteManager.Instance.CurrentPalette.rareOrganColor;
                break;
            case QualityType.Legendary:
                objCol = ColorPaletteManager.Instance.CurrentPalette.legendaryOrganColor;
                break;
            case QualityType.Epic:
                objCol = ColorPaletteManager.Instance.CurrentPalette.epicOrganColor;
                break;

            default:
                objCol = Color.white;
                break;
        }

        organImage.color = objCol;
    }

    private void SetText()
    {
        if (statsText == null) return;
        statsText.text = $"{GetStat(StatType.Mind)}\n" +
                         $"{GetStat(StatType.Soul)}\n" +
                         $"{GetStat(StatType.Body)}";
    }

    private void SetOutline()
    {
        if (outline != null)
        {
            outline.effectColor = ColorPaletteManager.Instance.CurrentPalette.outlineColor;
        }
    }
}