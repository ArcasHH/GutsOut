using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KnifeItem : DraggableItem, IPointerEnterHandler, IPointerExitHandler
{
    private Outline outline;
    public KnifeController parentController;
    

    public override ItemType Type => ItemType.HumanDeleter;
    public override CategoryType CategoryType => CategoryType.None;

    private int currentKnifeCost;
    private int knifeCostIncrease;

    protected override void Start()
    {
        base.Start();
        outline = GetComponent<Outline>();
        HideOutline();
        
        currentKnifeCost = GameManager.Instance.humanDeleterBaseCost;
        knifeCostIncrease = GameManager.Instance.humanDeleterCostIncrease;
        parentController.UpdateKnifeUI(currentKnifeCost);
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowOutline();
    public void OnPointerExit(PointerEventData eventData) => HideOutline();

    protected override void HandleDrop(PointerEventData eventData)
    {
        GameObject targetGo = eventData.pointerCurrentRaycast.gameObject;

        bool success = HandleKnifeDrop(targetGo);

        if (success)
            AudioManager.Instance.PlayRandomSoundFromFolder("Audio/Voices");
        ReturnToSource();
    }

    public bool HandleKnifeDrop(GameObject dropTarget)
    {
        if (dropTarget == null) return false;

        bool is_kill = GameManager.Instance.KillTargetHuman(dropTarget, currentKnifeCost);
        if (is_kill)
            UpdateKnifeCost();
        return is_kill;
    }

    
    private void UpdateKnifeCost()
    {
        currentKnifeCost += knifeCostIncrease;
        parentController.UpdateKnifeUI(currentKnifeCost);
    }
    
    private void ShowOutline()
    {
        if (GameManager.Instance.TotalScore < currentKnifeCost)
        {
            return;
        }
        if (outline != null) outline.enabled = true;
    }

    private void HideOutline()
    {
        if (outline != null) outline.enabled = false;
    }
}