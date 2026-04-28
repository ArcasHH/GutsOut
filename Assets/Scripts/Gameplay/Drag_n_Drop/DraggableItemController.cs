using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class DraggableItemController : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    public ItemType Type;
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Canvas canvas;

    public float dragThreshold = 5f;
    public float returnAnimDuration = 0.25f;

    public bool IsAnimating { get; private set; }

    private SlotController currentSlot;
    private SlotController sourceSlot;
    private RectTransform canvasRect;
    private bool isDragging = false;

    private void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        currentSlot = GetComponentInParent<SlotController>();
    }

    public void SetSlot(SlotController slot) => currentSlot = slot;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsAnimating) return;
        sourceSlot = currentSlot;
        isDragging = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (IsAnimating) return;

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

        if (Type == ItemType.HumanDeleter)
        {
            if (DayManager.Instance != null)
            {
                bool success = DayManager.Instance.HandleHumanDeleterDrop(targetGo, this);
                if (!success) ReturnToSource();
            }
            else ReturnToSource();
            return;
        }

        if (targetSlot != null && targetSlot != sourceSlot)
        {
            if (targetSlot.CanAccept(this.Type))
            {
                if (targetSlot.IsEmpty)
                {
                    targetSlot.PlaceItem(this);
                    return;
                }
                else if (sourceSlot != null && sourceSlot.CanAccept(targetSlot.CurrentItem.Type))
                {
                    DraggableItemController otherItem = targetSlot.CurrentItem;
                    StartCoroutine(PerformSwap(targetSlot, otherItem, sourceSlot));
                    return;
                }
            }
        }

        if (!IsAnimating) ReturnToSource();
    }

    private void ReturnToSource()
    {
        if (sourceSlot == null || IsAnimating) return;
        StartCoroutine(SmoothReturnToSlot(sourceSlot));
    }

    private IEnumerator SmoothReturnToSlot(SlotController targetSlot)
    {
        IsAnimating = true;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f;

        Vector2 targetPos = GetSlotCenterInCanvas(targetSlot.rectTransform);
        Vector2 startPos = rectTransform.anchoredPosition;

        float elapsed = 0f;
        while (elapsed < returnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnAnimDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3);
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        canvasGroup.blocksRaycasts = true;
        targetSlot.PlaceItem(this);
        IsAnimating = false;
    }

    private IEnumerator PerformSwap(SlotController targetSlot, DraggableItemController otherItem, SlotController sourceSlot)
    {
        IsAnimating = true;
        otherItem.IsAnimating = true;

        Vector3 startPosThis = rectTransform.position;
        Vector3 startPosOther = otherItem.rectTransform.position;

        Vector3 targetPosThis = targetSlot.rectTransform.position;
        Vector3 targetPosOther = sourceSlot.rectTransform.position;

        canvasGroup.blocksRaycasts = false;
        if (otherItem.canvasGroup != null) otherItem.canvasGroup.blocksRaycasts = false;

        float elapsed = 0f;
        while (elapsed < returnAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / returnAnimDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3); // Ease-out cubic

            rectTransform.position = Vector3.Lerp(startPosThis, targetPosThis, ease);
            otherItem.rectTransform.position = Vector3.Lerp(startPosOther, targetPosOther, ease);

            yield return null;
        }

        rectTransform.position = targetPosThis;
        otherItem.rectTransform.position = targetPosOther;

        targetSlot.PlaceItem(this);
        sourceSlot.PlaceItem(otherItem);

        canvasGroup.blocksRaycasts = true;
        if (otherItem.canvasGroup != null) otherItem.canvasGroup.blocksRaycasts = true;

        IsAnimating = false;
        otherItem.IsAnimating = false;
    }

    private Vector2 GetSlotCenterInCanvas(RectTransform slotRect)
    {
        Vector2 localPos;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, slotRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
        return localPos;
    }
}