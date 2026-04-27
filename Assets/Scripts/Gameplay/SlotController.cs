using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class SlotController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки слота")]
    public ItemType RequiredType = ItemType.None;
    public Image SlotImage;

    public static event Action OnInventoryChanged;

    public DraggableItemController CurrentItem { get; private set; }
    public bool IsEmpty => CurrentItem == null;

    public RectTransform rectTransform;
    private Color baseColor;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (SlotImage != null) baseColor = SlotImage.color;

        // 🔑 FIX #3: Инициализация при старте. Если предмет уже в слоте (расставлен в редакторе),
        // связываем их сразу, иначе первый дроп будет считать слот пустым.
        foreach (Transform child in transform)
        {
            DraggableItemController item = child.GetComponent<DraggableItemController>();
            if (item != null)
            {
                CurrentItem = item;
                item.SetSlot(this);
                break;
            }
        }
    }

    public bool CanAccept(ItemType type) => RequiredType == ItemType.None || type == RequiredType;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        DraggableItemController dragged = eventData.pointerDrag.GetComponent<DraggableItemController>();
        if (dragged != null) SetHighlight(CanAccept(dragged.Type));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (SlotImage != null) SlotImage.color = baseColor;
    }

    private void SetHighlight(bool isValid)
    {
        if (SlotImage == null) return;
        SlotImage.color = isValid ? new Color(0.5f, 1f, 0.5f, 0.4f) : new Color(1f, 0.3f, 0.3f, 0.4f);
    }

    public DropResult TryAccept(DraggableItemController draggedItem, SlotController sourceSlot)
    {
        if (!CanAccept(draggedItem.Type)) return DropResult.TypeMismatch;
        
        if (IsEmpty) 
        {
            PlaceItem(draggedItem);
            return DropResult.Success;
        }

        if (sourceSlot != null && sourceSlot.CanAccept(CurrentItem.Type))
        {
            DraggableItemController oldItem = CurrentItem;
            PlaceItem(draggedItem);
            sourceSlot.PlaceItem(oldItem);
            return DropResult.Swap;
        }

        return DropResult.Occupied;
    }

    public void PlaceItem(DraggableItemController item)
    {
        CurrentItem = item;
        item.SetSlot(this);
        item.transform.SetParent(rectTransform, false);
        item.transform.SetAsLastSibling();
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.canvasGroup.blocksRaycasts = true;
        item.canvasGroup.alpha = 1f;

        OnInventoryChanged?.Invoke();
    }

    public void ClearItem()
    {
        CurrentItem = null;
        OnInventoryChanged?.Invoke();
    }
}