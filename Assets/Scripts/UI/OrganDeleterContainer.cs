using UnityEngine;
using UnityEngine.UI;

public class OrganDeleterContainer : MonoBehaviour
{
    [SerializeField] private Image trashImage;

    private Color trashActiveColor;
    private Color trashColor;

    void Start()
    {
        SubscribeDependencies();
        trashActiveColor = ColorPaletteManager.Instance.CurrentPalette.cursedOrganColor;
        trashColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDragOrgan += SetColor;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDragOrgan -= SetColor;
    }

    private void SetColor(bool is_dragging)
    {
        trashImage.color = is_dragging ? trashActiveColor : trashColor;
    }
    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
