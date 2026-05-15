using TMPro;
using UnityEngine;

public class MobileOrganStatsPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text mindCountText;
    [SerializeField] private TMP_Text soulCountText;
    [SerializeField] private TMP_Text BodyCountText;

    private Color badColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
    private Color normColor = Color.white;
    void Start()
    {
        SubscribeDependencies();
        SetActivePanel(false);
    }
    private void SubscribeDependencies()
    {
        EventBus.OnDragOrgan += SetActivePanel;
        EventBus.OnDragOrganStats += SetText;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDragOrgan -= SetActivePanel;
        EventBus.OnDragOrganStats -= SetText;
    }

    private void SetText(int mind, int soul, int body)
    {
        SetStatText(mindCountText, mind);
        SetStatText(soulCountText, soul);
        SetStatText(BodyCountText, body);
    }

    private void SetStatText(TMP_Text textComponent, int value)
    {
        string sign = value > 0 ? "+" : "";
        textComponent.text = sign + value.ToString();
        textComponent.color = value > 0 ? normColor : badColor;
    }
    private void SetActivePanel(bool is_dragging)
    {
        gameObject.SetActive(is_dragging);
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
