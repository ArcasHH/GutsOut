using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ContainerSwapButton : MonoBehaviour
{
    //[SerializeField] private OrganStatsSummarizer containerA;
    [SerializeField] private GameObject ObjectWithHumanA;
    [SerializeField] private GameObject ObjectWithHumanB;

    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(OnSwapClicked);
    }

    private void OnSwapClicked()
    {
        OrganStatsSummarizer containerA = ObjectWithHumanA.GetComponentInChildren<OrganStatsSummarizer>();
        OrganStatsSummarizer containerB = ObjectWithHumanB.GetComponentInChildren<OrganStatsSummarizer>(); 
        if (containerA == null || containerB == null) return;
        SwapContainers(containerA, containerB);
    }

    private void SwapContainers(OrganStatsSummarizer contA, OrganStatsSummarizer contB)
    {
        var slotsA = contA.GetComponentsInChildren<SlotController>(true);
        var slotsB = contB.GetComponentsInChildren<SlotController>(true);

        int count = Mathf.Min(slotsA.Length, slotsB.Length);
        for (int i = 0; i < count; i++)
        {
            SwapSlots(slotsA[i], slotsB[i]);
        }

        EventBus.TriggerInventoryChanged();
    }

    private void SwapSlots(SlotController slotA, SlotController slotB)
    {
        if (slotA.CurrentItem == slotB.CurrentItem) return;

        var itemA = slotA.CurrentItem;
        var itemB = slotB.CurrentItem;

        slotA.CurrentItem = itemB;
        slotB.CurrentItem = itemA;

        if (itemA != null) BindItemToSlot(itemA, slotB);
        if (itemB != null) BindItemToSlot(itemB, slotA);
    }

    private void BindItemToSlot(DraggableItem item, SlotController slot)
    {
        item.SetSlot(slot);
        item.transform.SetParent(slot.rectTransform, false);
        item.transform.SetAsLastSibling();
        item.rectTransform.anchoredPosition = Vector2.zero;
        item.canvasGroup.blocksRaycasts = true;
        item.canvasGroup.alpha = 1f;
    }
}