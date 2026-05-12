using UnityEngine;
using UnityEngine.EventSystems;

public abstract class DraggableItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] protected float dragThreshold = 5f;
    [SerializeField] protected float returnAnimDuration = 0.25f;

    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public CanvasGroup canvasGroup;
    [HideInInspector] public Canvas canvas;

    public bool IsAnimating { get; protected set; }
    public abstract ItemType Type { get; }
    public abstract CategoryType CategoryType { get; }

    protected SlotController currentSlot;
    protected SlotController sourceSlot;
    protected RectTransform canvasRect;
    protected bool isDragging = false;

    protected virtual void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();
        currentSlot = GetComponentInParent<SlotController>();
    }

    public void SetSlot(SlotController slot) => currentSlot = slot;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        if (IsAnimating) return;
        sourceSlot = currentSlot;
        isDragging = false;
    }

    public virtual void OnDrag(PointerEventData eventData)
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

    protected virtual void StartDragging(PointerEventData eventData)
    {
        isDragging = true;
        sourceSlot?.ClearItem();

        AudioManager.Instance.PlaySound(AudioManager.SoundType.StartDragging);

        Vector2 mouseInCanvas;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, canvas.worldCamera, out mouseInCanvas);

        transform.SetParent(canvas.transform, false);
        transform.SetAsLastSibling();
        rectTransform.anchoredPosition = mouseInCanvas;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
        EventBus.TriggerInventoryChanged();
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        AudioManager.Instance.PlaySound(AudioManager.SoundType.EndDragging);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        HandleDrop(eventData);
    }

    protected abstract void HandleDrop(PointerEventData eventData);

    protected virtual void ReturnToSource()
    {
        if (sourceSlot == null || IsAnimating) return;
        StartCoroutine(SmoothReturnToSlot(sourceSlot));
    }

    protected System.Collections.IEnumerator SmoothReturnToSlot(SlotController targetSlot)
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

    public System.Collections.IEnumerator PerformSwap(SlotController targetSlot, DraggableItem otherItem, SlotController sourceSlot)
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
            float ease = 1f - Mathf.Pow(1f - t, 3);

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

    protected Vector2 GetSlotCenterInCanvas(RectTransform slotRect)
    {
        Vector2 localPos;
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, slotRect.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, canvas.worldCamera, out localPos);
        return localPos;
    }
}