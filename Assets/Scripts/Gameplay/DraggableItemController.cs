using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItemController : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public ItemType Type;
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Canvas canvas;

    [Header("Настройки")]
    [Tooltip("Минимальное смещение (в пикселях) для начала перетаскивания")]
    public float dragThreshold = 5f;

    private SlotController currentSlot;
    private SlotController sourceSlot;
    private RectTransform canvasRect;
    private bool isDragging = false;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        currentSlot = GetComponentInParent<SlotController>();
    }

    public void SetSlot(SlotController slot) => currentSlot = slot;

    public void OnPointerDown(PointerEventData eventData)
    {
        sourceSlot = currentSlot;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
        {
            float moveDistance = (eventData.position - eventData.pressPosition).sqrMagnitude;
            if (moveDistance > dragThreshold * dragThreshold)
                StartDragging(eventData);
            
            return;
        }

        Vector2 mouseInCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvas.worldCamera, out mouseInCanvas);
        rectTransform.anchoredPosition = mouseInCanvas;
    }

    private void StartDragging(PointerEventData eventData)
    {
        isDragging = true;
        sourceSlot?.ClearItem();

        Vector2 mouseInCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvas.worldCamera, out mouseInCanvas);

        transform.SetParent(canvas.transform, false);
        transform.SetAsLastSibling();

        rectTransform.anchoredPosition = mouseInCanvas;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        GameObject targetGo = eventData.pointerCurrentRaycast.gameObject;
        SlotController targetSlot = targetGo?.GetComponentInParent<SlotController>();

        bool handled = false;
        if (targetSlot != null && targetSlot != sourceSlot)
        {
            DropResult result = targetSlot.TryAccept(this, sourceSlot);
            handled = (result == DropResult.Success || result == DropResult.Swap);
        }

        if (!handled) ReturnToSource();
    }

    private void ReturnToSource()
    {
        if (sourceSlot != null)
            sourceSlot.PlaceItem(this);
    }
}