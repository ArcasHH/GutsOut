using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OrganItem : DraggableItem, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Organ Settings")]
    [SerializeField] private ObjectType organType;
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TMP_Text statsText;

    private GameOrgan data;
    [SerializeField] private Image organImage;
    [SerializeField] private Outline organOutline;
    //private Outline outline;
    [Header("Animation Parameters")]
    [SerializeField] private string isDraggingParam = "isDragging";
    [SerializeField] private Animator animator;

    public override ItemType Type => ConvertToItemType(organType);
    public override CategoryType CategoryType => data?.category_type ?? CategoryType.None;
    public GameOrgan Data => data;

    private string spritesPath = "Sprites/GameSprites/Organs";

    protected override void Start()
    {
        base.Start();

        if (OrganRandomizer.Instance != null)
            data = OrganRandomizer.Instance.GetRandomOrgan(organType);

        if (data == null)
        {
            Debug.LogWarning($"[{name}] No data selected for type {organType}!");
            return;
        }

        //organImage = GetComponent<Image>();
        //organOutline = GetComponent<Outline>();

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
        if (statsPanel != null) statsPanel.SetActive(true);
        if (organOutline != null) organOutline.enabled = true;
    }

    private void HideStatsPanel()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
        if (organOutline != null) organOutline.enabled = false;
    }

    protected override void HandleDrop(PointerEventData eventData)
    {
        GameObject targetGo = eventData.pointerCurrentRaycast.gameObject;
        SlotController targetSlot = targetGo?.GetComponentInParent<SlotController>();

        if (targetSlot && targetSlot.RequiredType == ItemType.OrganDeleter)
        {
            Destroy(gameObject);
            return;
        }

        if (targetSlot != null && targetSlot != sourceSlot)
        {
            if (targetSlot.CanAccept(this))
            {
                if (targetSlot.IsEmpty)
                {
                    targetSlot.PlaceItem(this);
                    return;
                }
                else if (sourceSlot != null && sourceSlot.CanAccept(targetSlot.CurrentItem))
                {
                    DraggableItem otherItem = targetSlot.CurrentItem;
                    StartCoroutine(PerformSwap(targetSlot, otherItem, sourceSlot));
                    return;
                }
            }
        }

        if (!IsAnimating) ReturnToSource();
    }

    protected override void StartDragging(PointerEventData eventData)
    {
        base.StartDragging(eventData);
        EventBus.TriggerDragOrgan(true);
        SetDraggingAnim(true);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        EventBus.TriggerDragOrgan(false);
        SetDraggingAnim(false);
    }

    private ItemType ConvertToItemType(ObjectType objectType)
    {
        return objectType switch
        {
            ObjectType.Heart => ItemType.Heart,
            ObjectType.Brain => ItemType.Brain,
            ObjectType.Lungs => ItemType.Lungs,
            ObjectType.Gut => ItemType.Guts,
            _ => ItemType.None
        };
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
            organImage.sprite = targetSprite;
        else
            Debug.LogWarning($"Sprite not found: {targetSpriteName} in {spritesPath}");
    }

    private void SetColor()
    {
        if (organImage == null || !ColorPaletteManager.Instance) return;

        Color objCol = data.quality_type switch
        {
            QualityType.Cursed => ColorPaletteManager.Instance.CurrentPalette.cursedOrganColor,
            QualityType.Bad => ColorPaletteManager.Instance.CurrentPalette.badOrganColor,
            QualityType.Ordinary => ColorPaletteManager.Instance.CurrentPalette.ordinaryOrganColor,
            QualityType.Good => ColorPaletteManager.Instance.CurrentPalette.goodOrganColor,
            QualityType.Rare => ColorPaletteManager.Instance.CurrentPalette.rareOrganColor,
            QualityType.Epic => ColorPaletteManager.Instance.CurrentPalette.legendaryOrganColor,
            QualityType.Legendary => ColorPaletteManager.Instance.CurrentPalette.epicOrganColor,
            _ => Color.white
        };

        organImage.color = objCol;
    }

    private void SetText()
    {
        if (statsText == null) return;
        string mind = "";
        if (GetStat(StatType.Mind) > 0) mind = "+";
        string soul = "";
        if (GetStat(StatType.Soul) > 0) soul = "+";
        string body = "";
        if (GetStat(StatType.Body) > 0) body = "+";
        statsText.text = $"{mind}{GetStat(StatType.Mind)}\n{soul}{GetStat(StatType.Soul)}\n{body}{GetStat(StatType.Body)}";
    }

    private void SetOutline()
    {
        if (organOutline != null && ColorPaletteManager.Instance != null)
            organOutline.effectColor = ColorPaletteManager.Instance.CurrentPalette.outlineColor;
    }

    private void SetDraggingAnim(bool isDragging)
    {
        if (animator != null)
        {
            animator.SetBool(isDraggingParam, isDragging);
        }
    }
}