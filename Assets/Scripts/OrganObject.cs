using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // 🔑 Нужно для интерфейсов

public class OrganObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ObjectTypeData data;
    [SerializeField] private GameObject stats_panel;
    [SerializeField] private TMP_Text statsText;

    public ObjectTypeData Data => data;

    private void Awake()
    {
        if (data == null)
            Debug.LogWarning($"[{name}] No ObjectTypeData! ");

        SetText();
        HideStatsPanel();
    }

    // 🔑 Правильные сигнатуры методов интерфейса
    public void OnPointerEnter(PointerEventData eventData) => ShowStatsPanel();
    public void OnPointerExit(PointerEventData eventData) => HideStatsPanel();

    public int GetStat(StatType stat) => data?.GetStat(stat) ?? 0;

    private void ShowStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(true);
    }

    private void HideStatsPanel()
    {
        if (stats_panel != null) stats_panel.SetActive(false);
    }

    private void SetText()
    {
        if (statsText == null) return;
        statsText.text = $"{GetStat(StatType.Mind)}\n" +
                         $"{GetStat(StatType.Soul)}\n" +
                         $"{GetStat(StatType.Body)}";
    }
}