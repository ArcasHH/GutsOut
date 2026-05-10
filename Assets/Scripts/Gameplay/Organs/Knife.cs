using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class Knife : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Outline outline;
    void Start()
    {
        outline = GetComponent<Outline>();
        HideOutline();
    }

    public void OnPointerEnter(PointerEventData eventData) => ShowOutline();
    public void OnPointerExit(PointerEventData eventData) => HideOutline();

    private void ShowOutline()
    {
        if (outline == null) return;
        outline.enabled = true;
    }

    private void HideOutline()
    {
        if (outline == null) return;
        outline.enabled = false;
    }
}
