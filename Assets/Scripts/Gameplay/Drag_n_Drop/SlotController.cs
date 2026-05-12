using UnityEngine;
using UnityEngine.EventSystems;

public class SlotController : MonoBehaviour
{
    [Header("Slot Settings")]
    public ItemType RequiredType = ItemType.None;
    [SerializeField] private CategoryType categoryRestriction = CategoryType.None;

    public DraggableItem CurrentItem { get; set; }
    public bool IsEmpty => CurrentItem == null;
    public RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (categoryRestriction == CategoryType.None)
        {
            var summarizer = GetComponentInParent<OrganStatsSummarizer>();
            if (summarizer != null) categoryRestriction = summarizer.CollectionCategory;
        }

        foreach (Transform child in transform)
        {
            DraggableItem item = child.GetComponent<DraggableItem>();
            if (item != null)
            {
                CurrentItem = item;
                item.SetSlot(this);
                break;
            }
        }
    }

    public bool CanAccept(DraggableItem item)
    {
        if (item == null) return false;

        bool typeOk = RequiredType == ItemType.None || item.Type == RequiredType;
        bool categoryOk = categoryRestriction == CategoryType.None || item.CategoryType == categoryRestriction;

        return typeOk && categoryOk;
    }

    public bool CanAcceptTypeOnly(ItemType type) => RequiredType == ItemType.None || type == RequiredType;

    public void PlaceItem(DraggableItem item)
    {
        CurrentItem = item;
        item.SetSlot(this);
        item.transform.SetParent(rectTransform, false);
        item.transform.SetAsLastSibling();
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.canvasGroup.blocksRaycasts = true;
        item.canvasGroup.alpha = 1f;

        EventBus.TriggerInventoryChanged();
    }

    public void ClearItem()
    {
        CurrentItem = null;
        EventBus.TriggerInventoryChanged();
    }
}