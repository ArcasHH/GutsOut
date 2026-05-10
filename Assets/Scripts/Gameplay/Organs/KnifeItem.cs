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

    protected override void Start()
    {
        base.Start();
        outline = GetComponent<Outline>();
        HideOutline();
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowOutline();
    public void OnPointerExit(PointerEventData eventData) => HideOutline();

    protected override void HandleDrop(PointerEventData eventData)
    {
        GameObject targetGo = eventData.pointerCurrentRaycast.gameObject;

        bool success = parentController.HandleKnifeDrop(targetGo, this);//DayManager.Instance.HandleKnifeDrop(targetGo, this);
        if (!success)
                ReturnToSource();
        else AudioManager.Instance.PlayRandomSoundFromFolder("Audio/Voices");
    }


    private void ShowOutline()
    {
        if (outline != null) outline.enabled = true;
    }

    private void HideOutline()
    {
        if (outline != null) outline.enabled = false;
    }
}